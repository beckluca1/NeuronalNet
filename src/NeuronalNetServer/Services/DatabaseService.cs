using NeuronalNetServer.Helpers;
using MySql.Data.MySqlClient;


namespace NeuronalNetServer.Services
{
    public class DatabaseService
    {
        #region Fields

        private MySqlConnection _connection;

        #endregion

        #region Methods

        public void Dispose()
        {
            _connection.Close();
        }

        public void Initialize(string connectionString)
        {
            OpenConnection(connectionString);
        }

        public void GetAllTrafficSigns(int limit = 0)
        {
            //TODO: add select
        }

        public void GetTrafficSignType(TrafficSignType trafficSignType)
        {
            //TODO: add select
        }

        private void OpenConnection(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
        }

        #endregion
    }
}