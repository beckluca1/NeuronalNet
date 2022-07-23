using MySql.Data.MySqlClient;
using NeuronalNetServer.Proto;
using NeuralNet;

namespace NeuronalNetServer.Services
{
    public class DatabaseService
    {
        #region Fields

        const int BinaryDataLength = 46 * 46;
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

        public void GetNeuralNet(int identifier)
        {
            string sqlSelect = "select * from neural_net where id = @IDENTIFIER";

            MySqlParameter[] parameters = {
                new MySqlParameter("@IDENTIFIER", identifier),
            };

            MySqlCommand command = new MySqlCommand(sqlSelect, _connection);
            command.Parameters.AddRange(parameters);
            command.Prepare();

            var data = command.ExecuteReader();

            FillNetSaveStateHandler(data);

            command.Dispose();
        }

        public NeuralNetData GetLatestNeuralNet()
        {
            string sqlSelect = @"select net_data, rating from neural_net
                                 order by uploaded desc limit 1";

            MySqlCommand command = new MySqlCommand(sqlSelect, _connection);
            var dataReader = command.ExecuteReader();

            var netData = GetSingleNetData(dataReader);

            return netData;
        }

        public void InsertNeuralNet(ConvolutionalNet neuralNet, int rating)
        {
            string sqlInsert = @"insert into neural_net (net_data, rating, uploaded)
                                 values (@NET_DATA, @RATING, now())";

            MySqlParameter[] parameters = {
                new MySqlParameter("@NET_DATA", NetSaveStateHandler.saveFromNet(neuralNet)),
                new MySqlParameter("@RATING", rating)
            };

            MySqlCommand command = new MySqlCommand(sqlInsert, _connection);
            command.Parameters.AddRange(parameters);
            command.Prepare();

            command.ExecuteNonQuery();

            command.Dispose();
        }

        public List<TrafficSign> GetAllTrafficSigns(int limit = 0)
        {
            string sqlSelect = "select * from traffic_sign";

            MySqlCommand command = new MySqlCommand(sqlSelect, _connection);
            command.Prepare();

            var data = command.ExecuteReader();

            var floatBitmapList = BuildTrafficSignList(data);

            command.Dispose();

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

            command.Dispose();

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

            command.Dispose();
        }

        public NumberOfSigns GetNumberOfSigns()
        {
            string sqlSelect = @"select sign_type, count(*) number
                                 from traffic_sign number group by sign_type";

            MySqlCommand command = new MySqlCommand(sqlSelect, _connection);
            var dataReader = command.ExecuteReader();

            var numberOfSigns = ExtractSignNumberReader(dataReader);

            return numberOfSigns;
        }


        #endregion

        #region Private Methods

        private void OpenConnection(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
        }

        private void FillNetSaveStateHandler(MySqlDataReader reader)
        {
            using (reader)
            {
                if (!reader.HasRows)
                    return;

                while (reader.Read())
                {
                    uint size = reader.GetUInt32(reader.GetOrdinal("net_data"));
                    byte[] netData = new byte[size];

                    reader.GetBytes(reader.GetOrdinal("net_data"), 0, netData, 0, (int)size);

                    NetSaveStateHandler.DataList = Global.byteToFloat(netData).ToList<float>();
                }
            }
        }

        private NeuralNetData GetSingleNetData(MySqlDataReader reader)
        {
            NeuralNetData neuralNetData = new NeuralNetData
            {
                NetData = Google.Protobuf.ByteString.CopyFrom(new byte[0]),
                Rating = default
            };

            using (reader)
            {
                if (!reader.HasRows)
                    return neuralNetData;

                reader.Read();

                uint size = reader.GetUInt32(reader.GetOrdinal("net_data"));
                byte[] netData = new byte[size];
                int netRating;

                reader.GetBytes(reader.GetOrdinal("net_data"), 0, netData, 0, (int)size);
                netRating = reader.GetInt32(reader.GetOrdinal("rating"));

                neuralNetData.NetData = Google.Protobuf.ByteString.CopyFrom(netData);
                neuralNetData.Rating = netRating;
            }

            return neuralNetData;
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

        private NumberOfSigns ExtractSignNumberReader(MySqlDataReader reader)
        {
            var signNumbers = new Dictionary<string, int>();

            using (reader)
            {
                if (!reader.HasRows)
                {
                    var emptyNumberOfSigns = new NumberOfSigns()
                    {
                        Stop = 0,
                        ThirtySpeedLimit = 0,
                        FiftySpeedLimit = 0,
                        PriorityRoad = 0,
                        GiveWay = 0,
                        Unclassified = 0
                    };

                    return emptyNumberOfSigns;
                }

                while (reader.Read())
                {
                    signNumbers.Add(reader.GetString("sign_type"), reader.GetInt32("number"));
                }
            }

            return BuildNumberOfSigns(signNumbers);
        }

        private NumberOfSigns BuildNumberOfSigns(Dictionary<string, int> numberOfSignsDict)
        {
            int stop = 0;
            numberOfSignsDict.TryGetValue(SignType.Stop.ToString(), out stop);

            int thirtySpeedLimit = 0;
            numberOfSignsDict.TryGetValue(SignType.GiveWay.ToString(), out thirtySpeedLimit);

            int fiftySpeedLimit = 0;
            numberOfSignsDict.TryGetValue(SignType.PriorityRoad.ToString(), out fiftySpeedLimit);

            int priorityRoad = 0;
            numberOfSignsDict.TryGetValue(SignType.ThirtySpeedLimit.ToString(), out priorityRoad);

            int giveWay = 0;
            numberOfSignsDict.TryGetValue(SignType.FiftySpeedLimit.ToString(), out giveWay);

            int unclassified = 0;
            numberOfSignsDict.TryGetValue(SignType.Unclassified.ToString(), out unclassified);

            var numberOfSigns = new NumberOfSigns()
            {
                Stop = stop,
                ThirtySpeedLimit = thirtySpeedLimit,
                FiftySpeedLimit = fiftySpeedLimit,
                PriorityRoad = priorityRoad,
                GiveWay = giveWay,
                Unclassified = unclassified
            };

            return numberOfSigns;

        }

        #endregion

        #endregion
    }
}