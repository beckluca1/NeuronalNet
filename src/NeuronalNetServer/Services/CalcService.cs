using NeuralNet;
using NeuronalNetServer.Helpers;
using NeuronalNetServer.Proto;

namespace NeuronalNetServer.Services
{
    public class NeuralCalculator
    {
        #region Fields

        private Credentials _credentials = default!;
        private ConvolutionalNet _netCNN;
        private ProposalNeuralNet _netRPN;
        private readonly DatabaseService _dbService;

        public float CNNCost = 0;
        public int CNNCorrect = 0;
        public float RPNCost = 0;
        public int RPNCorrect = 0;

        #endregion

        #region Constructor

        public NeuralCalculator()
        {
            BuildConfiguration();

            _dbService = new DatabaseService();
            _dbService.Initialize(_credentials.DbConnectionString!);

            _netCNN = new ConvolutionalNet();
            _netRPN = new ProposalNeuralNet();
        }

        #endregion

        #region Methods

        public TrafficSign? LoadSignImage(int number)
        {
            SignType type = (SignType)number;

            List<TrafficSign> signs = _dbService.GetTrafficSignType(type);
            if(signs.Count==0)
            {
                Console.WriteLine("No sign exisiting");
                return null;
            }
            int randomIndex = new Random().Next() % signs.Count;

            if(number==2&&randomIndex==8)
            {
                byte[] arra = signs[randomIndex].Red.ToByteArray();
            }  

            return signs[randomIndex];
        }

        public TrafficImage? LoadTrafficImage()
        {
            List<TrafficImage> images = _dbService.GetAllTrafficImages();
            if(images.Count==0)
            {
                Console.WriteLine("No image exisiting");
                return null;
            }
            int randomIndex = new Random().Next() % images.Count;

            return images[randomIndex];
        }

        public void InitNet()
        {   
            NeuralNetData cnnData = _dbService.GetLatestCNN();
            if(cnnData.NetData.ToByteArray().Length!=0)
                _netCNN = NetSaveStateHandler.readFromSaveStateCNN(cnnData.NetData.ToByteArray());

            NeuralNetData rpnData = _dbService.GetLatestRPN();
            if(rpnData.NetData.ToByteArray().Length!=0)
                _netRPN = NetSaveStateHandler.readFromSaveStateRPN(rpnData.NetData.ToByteArray());
        }

        public void CalculateCNN(int number)
        {
            TrafficSign? loadedSign = LoadSignImage(number);
            if(loadedSign==null)
                return;    


            _netCNN.SetInput(loadedSign.Red.ToByteArray(), loadedSign.Green.ToByteArray(), loadedSign.Blue.ToByteArray());
            _netCNN.Update();

            List<float> trainingValues = new List<float>();
            for (int i = 0; i < 5; i++)
            {
                trainingValues.Add(i == number ? 1 : 0);
            }
            _netCNN.CalculateCost(trainingValues);
            _netCNN.Correct(number);
            _netCNN.CalculateChanges();
        }

        public void CalculateRPN()
        {
            TrafficImage? loadedImage = LoadTrafficImage();
            if(loadedImage==null)
                return;

            List<Rectangle> realRectangles = new List<Rectangle>();

            int signCount = (int)loadedImage.SignCount;
            byte[] locationData = loadedImage.Location.ToByteArray();
            for(int i=0;i<signCount;i++)
            {
                realRectangles.Add(new Rectangle(((float)locationData[i*4+0])/255.0f,((float)locationData[i*4+1])/255.0f,((float)locationData[i*4+2])/255.0f,((float)locationData[i*4+3])/255.0f));
            }


            _netRPN.SetInput(loadedImage.Red.ToByteArray(), loadedImage.Green.ToByteArray(), loadedImage.Blue.ToByteArray());
            _netRPN.Update();

            int bestIndex = realRectangles[0].GetBestRectangle(_netRPN.allRectangles);

            //Console.Write(bestIndex+", ");

            List<float> trainingValues = new List<float>();
            for(int i=0;i<_netRPN.neuralMaps[_netRPN.neuralMaps.Count-1][0].mapSize;i++)
            {
                float iou = _netRPN.allRectangles[i].GIOU(realRectangles[0]);
                float dLoss = i==bestIndex||iou>0.7 ? 1 : iou < 0.3 ? 0 : _netRPN.neuralMaps[_netRPN.neuralMaps.Count-1][0].values[0];
                trainingValues.Add(dLoss);//_netRPN.allRectangles[i].GIOU(realRectangles[0]));
            }

            _netRPN.CalculateCost(trainingValues);
            _netRPN.presesntBest();
            _netRPN.Correct(bestIndex);
            _netRPN.CalculateChanges();
        }

        public void uploadCurrentNet()
        {
            _dbService.InsertCNN(_netCNN,(int)(CNNCorrect/Global.BATCH_SIZE*100));
            _dbService.InsertRPN(_netRPN,(int)(RPNCorrect/Global.BATCH_SIZE*100));
        }

        public void ImproveCNN()
        {
            _netCNN.Improve();
            CNNCost = _netCNN.cost;
            _netCNN.cost = 0;
            CNNCorrect = _netCNN.correct;
            _netCNN.correct = 0;

            Console.WriteLine(CNNCost/Global.BATCH_SIZE + "; "+(((float)CNNCorrect)/Global.BATCH_SIZE*100)+"%");
        }

        public void ImproveRPN()
        {
            _netRPN.Improve();
            RPNCost = _netRPN.cost;
            _netRPN.cost = 0;
            RPNCorrect = _netRPN.correct;
            _netRPN.correct = 0;

            Console.WriteLine(RPNCost/Global.BATCH_SIZE + "; "+(((float)RPNCorrect)/Global.BATCH_SIZE*100)+"%");
        }

        private void BuildConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>(optional: true)
                .AddEnvironmentVariables()
                .Build();
            var neuroSection = config.GetSection("NEURO");

            _credentials = new Credentials
            {
                DbConnectionString = neuroSection["DB_CONNECTION_STRING"],
            };
        }

        #endregion
    }
}
