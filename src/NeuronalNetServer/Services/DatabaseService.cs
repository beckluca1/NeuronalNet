using NeuralNet.Helpers;
using NeuronalNetServer.Helpers;
using MySql.Data.MySqlClient;


namespace NeuronalNetServer.Services
{
    public class DatabaseService
    {
        #region Fields

        const int BinaryDataLength = 46 * 4;
        private MySqlConnection _connection;

        #endregion

        #region Methods

        #region Public Methods

        public void Dispose()
        {
            _connection.Close();
        }

        public void Initialize(string connectionString)
        {
            OpenConnection(connectionString);
        }

        public List<FloatBitmapData> GetAllTrafficSigns(int limit = 0)
        {
            string sqlSelect = "select * from traffic_sign";

            MySqlCommand command = new MySqlCommand(sqlSelect, _connection);
            command.Prepare();

            var data = command.ExecuteReader();

            var floatBitmapList = ConvertTrafficSignReader(data);

            return floatBitmapList;
        }

        public List<FloatBitmapData> GetTrafficSignType(TrafficSignType trafficSignType)
        {
            string sqlSelect = "select * from traffic_sign where sign_type = @TYPE";

            MySqlCommand command = new MySqlCommand(sqlSelect, _connection);
            command.Parameters.AddWithValue("@TYPE", ""); //TODO: convert sign type
            command.Prepare();

            var data = command.ExecuteReader();

            var floatBitmapList = ConvertTrafficSignReader(data);

            return floatBitmapList;
        }

        public void InsertTrafficSign(TrafficSignType trafficSignType, FloatBitmapData bitmapData)
        {
            var binaryBitmap = bitmapData.ConvertToBinary();

            if (binaryBitmap == null)
                return;

            string sqlInsert = @"insert into traffic_sign (sign_type, red_data, green_data, blue_data, uploaded)
                                 values (@TYPE, @RED, @GREEN, @BLUE now())";

            MySqlParameter[] parameters = {
                new MySqlParameter("@TYPE", ""),
                new MySqlParameter("@RED", binaryBitmap.RedData),
                new MySqlParameter("@GREEN", binaryBitmap.GreenData),
                new MySqlParameter("@BLUE", binaryBitmap.BlueData)
            };                                 

            MySqlCommand command = new MySqlCommand(sqlInsert, _connection);
            command.Parameters.AddRange(parameters);
            command.Prepare();

            command.ExecuteNonQuery();
        }

        #endregion

        #region Private Methods

        private void OpenConnection(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
        }

        private List<FloatBitmapData> ConvertTrafficSignReader(MySqlDataReader reader)
        {
            var trafficSignList = new List<FloatBitmapData>();

            using (reader)
            {
                if (!reader.HasRows)
                return trafficSignList;

                while (reader.Read())
                {
                    byte[] redData = new byte[BinaryDataLength];
                    byte[] greenData = new byte[BinaryDataLength];
                    byte[] blueData = new byte[BinaryDataLength];

                    reader.GetBytes(reader.GetOrdinal("red_data"), 0, redData, 0, BinaryDataLength);
                    reader.GetBytes(reader.GetOrdinal("green_data"), 0, greenData, 0, BinaryDataLength);
                    reader.GetBytes(reader.GetOrdinal("blue_data"), 0, blueData, 0, BinaryDataLength);

                    //TODO: convert sign type
                    var binaryBitmap = new BinaryBitmapData(redData, greenData, blueData);

                    trafficSignList.Add(binaryBitmap.ConvertToFloat());
                }
            }
            return trafficSignList;
        }

        #endregion

        #endregion
    }
}