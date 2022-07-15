using NeuralNet;
using NeuronalNetServer.Helpers;
using NeuronalNetServer.Proto;

namespace NeuronalNetServer.Services
{
    public class NeuralCalculator
    {
        #region Fields

        private Credentials _credentials = default!;
        private ComplexNeuralNet _net;
        private readonly DatabaseService _dbService;

        #endregion

        #region Constructor

        public NeuralCalculator()
        {
            BuildConfiguration();

            _dbService = new DatabaseService();
            _dbService.Initialize(_credentials.DbConnectionString!);

            _net = new ComplexNeuralNet();
        }

        #endregion

        #region Methods

        public TrafficSign? LoadImage(int number)
        {
            SignType type = (SignType)number;

            List<TrafficSign> signs = _dbService.GetTrafficSignType(type);
            if(signs.Count==0)
                return null;
            int randomIndex = new Random().Next() % signs.Count;

            return signs[randomIndex];
        }

        public void Calculate(int number)
        {
            TrafficSign? loadedSign = LoadImage(number);
            if(loadedSign==null)
                return;

            List<float> trainingValues = new List<float>();

            _net.Update(loadedSign.Red.ToByteArray(), loadedSign.Green.ToByteArray(), loadedSign.Blue.ToByteArray());
            for (int i = 0; i < 5; i++)
            {
                trainingValues.Add(i == number ? 1 : 0);
                //Console.Write((int)(_net.getOutput()[i]*100000)+", ");
            }
            float difference =_net.CalculateChanges(trainingValues);
            Console.WriteLine(difference + (_net.Correct(number) ? " Correct " : " Incorrect ") + ((SignType)number).ToString());
        }

        public void Improve()
        {
            _net.Improve();
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
