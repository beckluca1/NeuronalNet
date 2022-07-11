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

        public TrafficSign LoadImage(int number)
        {
            int maxIndex = 12;
            int randomIndex = new Random().Next() % maxIndex;

            TrafficSign sign = new TrafficSign();

            //Load image

            return sign;
        }

        public void Calculate(int number, List<float> dataRed, List<float> dataGreen, List<float> dataBlue)
        {
            TrafficSign loadedSign = LoadImage(number);

            //Convert sign to image data

            List<float> trainingValues = new List<float>();
            for (int i = 0; i <= 10; i++)
            {
                trainingValues.Add(i == number ? 1 : 0);
            }
            _net.Update(dataRed, dataGreen, dataBlue);
            _net.CalculateChanges(trainingValues);
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
