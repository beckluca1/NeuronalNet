using NeuralNet;
using NeuronalNetServer.Helpers;

namespace NeuronalNetServer.Services
{
    public class NeuralCalculator
    {
        #region Fields

        private Credentials _credentials;

        private IServiceProvider _services;

        private ComplexNeuralNet _net;

        #endregion

        #region Constructor

        public NeuralCalculator()
        {
            ConfigureServices();
            InitializeServices();
        }

        #endregion

        #region Methods

        public void Calculate(int number, List<float> dataRed, List<float> dataGreen, List<float> dataBlue)
        {
            List<float> trainingValues = new List<float>();
            for (int i = 0; i <= 10; i++)
            {
                trainingValues.Add(i == number ? 1 : 0);
            }
            _net.Update(dataRed,dataGreen,dataBlue);
            _net.CalculateChanges(trainingValues);
            _net.Improve();
        }

        private void ConfigureServices()
        {
            _services = new ServiceCollection()
            .AddSingleton<DatabaseService>()
            .AddSingleton<ComplexNeuralNet>()
            .BuildServiceProvider();
        }

        private void InitializeServices()
        {
            _net = _services.GetRequiredService<ComplexNeuralNet>();

            var dbService = _services.GetRequiredService<DatabaseService>();

            if (_credentials.DbConnectionString == null)
                return;
                //throw new Exception("No database connection string has been provided.");

            dbService.Initialize(_credentials.DbConnectionString);
        }

        private void GetConfiguration()
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
