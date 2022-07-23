using NeuralNet;
using NeuronalNetServer.Helpers;
using NeuronalNetServer.Proto;

namespace NeuronalNetServer.Services
{
    public class NeuralCalculator
    {
        #region Fields

        private Credentials _credentials = default!;
        private ConvolutionalNet _net;
        private readonly DatabaseService _dbService;

        public float cost = 0;
        public int correct = 0;

        #endregion

        #region Constructor

        public NeuralCalculator()
        {
            BuildConfiguration();

            _dbService = new DatabaseService();
            _dbService.Initialize(_credentials.DbConnectionString!);

            _net = new ConvolutionalNet();
        }

        #endregion

        #region Methods

        public TrafficSign? LoadImage(int number)
        {
            SignType type = (SignType)number;

            List<TrafficSign> signs = _dbService.GetTrafficSignType(type);
            if(signs.Count==0)
            {
                Console.WriteLine("No net exisiting");
                return null;
            }
            int randomIndex = new Random().Next() % signs.Count;

            return signs[randomIndex];
        }

        public void InitNet()
        {   
            NeuralNetData netData = _dbService.GetLatestNeuralNet();
            if(netData.Rating==0)
                return;
            _net = NetSaveStateHandler.readFromSaveState(_dbService.GetLatestNeuralNet().NetData.ToByteArray());

        }

        public void Calculate(int number)
        {
            TrafficSign? loadedSign = LoadImage(number);
            if(loadedSign==null)
                return;

            List<float> trainingValues = new List<float>();

            _net.SetInput(loadedSign.Red.ToByteArray(), loadedSign.Green.ToByteArray(), loadedSign.Blue.ToByteArray());
            _net.Update();

            for (int i = 0; i < 5; i++)
            {
                trainingValues.Add(i == number ? 1 : 0);
            }
            _net.CalculateCost(trainingValues);
            _net.Correct(number);
            _net.CalculateChanges();
        }

        public void uploadCurrentNet()
        {
            _dbService.InsertNeuralNet(_net,correct*10);
        }

        public void Improve()
        {
            _net.Improve();
            cost = _net.cost;
            _net.cost = 0;
            correct = _net.correct;
            _net.correct = 0;

            Console.WriteLine(cost/10.0f + "; "+(((float)correct)/10.0f*100)+"%");
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
