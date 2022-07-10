using NeuronalNetServer.Helpers;
using MySql.Data.MySqlClient;
using NeuronalNetServer.Proto;

namespace NeuronalNetServer.Services
{
    public class DatabaseService
    {
        #region Fields

        const int BinaryDataLength = 46 * 4;
        private MySqlConnection _connection = default!;

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

        public List<TrafficSign> GetAllTrafficSigns(int limit = 0)
        {
            string sqlSelect = "select * from traffic_sign";

            MySqlCommand command = new MySqlCommand(sqlSelect, _connection);
            command.Prepare();

            var data = command.ExecuteReader();

            var floatBitmapList = BuildTrafficSignList(data);

            return floatBitmapList;
        }

        public List<TrafficSign> GetTrafficSignType(SignType trafficSignType)
        {
            string sqlSelect = "select * from traffic_sign where sign_type = @TYPE";

            MySqlCommand command = new MySqlCommand(sqlSelect, _connection);
            command.Parameters.AddWithValue("@TYPE", trafficSignType.ToString());
            command.Prepare();

            var data = command.ExecuteReader();

            var floatBitmapList = BuildTrafficSignList(data);

            return floatBitmapList;
        }

        public void InsertTrafficSign(TrafficSign trafficSign)
        {
            string sqlInsert = @"insert into traffic_sign (sign_type, red_data, green_data, blue_data, uploaded)
                                 values (@TYPE, @RED, @GREEN, @BLUE, now())";

            MySqlParameter[] parameters = {
                new MySqlParameter("@TYPE", trafficSign.SignType.ToString()),
                new MySqlParameter("@RED", trafficSign.Red.ToByteArray()),
                new MySqlParameter("@GREEN", trafficSign.Green.ToByteArray()),
                new MySqlParameter("@BLUE", trafficSign.Blue.ToByteArray())
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

        private List<TrafficSign> BuildTrafficSignList(MySqlDataReader reader)
        {
            var trafficSignList = new List<TrafficSign>();

            using (reader)
            {
                if (!reader.HasRows)
                return trafficSignList;

                while (reader.Read())
                {
                    SignType signType = ConvertStringToSignType(reader.GetString("sign_type"));

                    byte[] redData = new byte[BinaryDataLength];
                    byte[] greenData = new byte[BinaryDataLength];
                    byte[] blueData = new byte[BinaryDataLength];

                    reader.GetBytes(reader.GetOrdinal("red_data"), 0, redData, 0, BinaryDataLength);
                    reader.GetBytes(reader.GetOrdinal("green_data"), 0, greenData, 0, BinaryDataLength);
                    reader.GetBytes(reader.GetOrdinal("blue_data"), 0, blueData, 0, BinaryDataLength);

                    var trafficSign = new TrafficSign()
                    {
                        SignType = signType,
                        Red = Google.Protobuf.ByteString.CopyFrom(redData),
                        Green = Google.Protobuf.ByteString.CopyFrom(greenData),
                        Blue = Google.Protobuf.ByteString.CopyFrom(blueData)
                    };

                    trafficSignList.Add(trafficSign);
                }
            }
            return trafficSignList;
        }

        private SignType ConvertStringToSignType(string signType)
        {
            SignType enumSignType = SignType.Unclassified;

            Enum.TryParse(signType, out enumSignType);

            return enumSignType;
        }

        #endregion

        #endregion
    }
}