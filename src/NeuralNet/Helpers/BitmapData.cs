using NeuronalNetServer.Helpers;

namespace NeuralNet.Helpers
{
    #region Float Bitmap Class

    public class FloatBitmapData
    {
        #region Constructors
        
        public FloatBitmapData(List<float> redData, List<float> greenData, List<float> blueData)
        {
            RedData = redData;
            GreenData = greenData;
            BlueData = blueData;
            SignType = TrafficSignType.Unclassified;
        }

        public FloatBitmapData(List<float> redData, List<float> greenData, List<float> blueData, TrafficSignType signType)
        {
            RedData = redData;
            GreenData = greenData;
            BlueData = blueData;
            SignType = signType;
        }

        #endregion

        #region Properties

        public TrafficSignType SignType;
        public List<float> RedData { get; set; }
        public List<float> GreenData { get; set; }
        public List<float> BlueData { get; set; }

        #endregion

        #region Methods

        public BinaryBitmapData? ConvertToBinary()
        {

            var redBinary = ToByteArray(RedData);
            var greenBinary = ToByteArray(GreenData);
            var blueBinary = ToByteArray(BlueData);

            var binaryBitmap = new BinaryBitmapData(redBinary, greenBinary, blueBinary);

            return binaryBitmap;
        }


        private byte[] ToByteArray(List<float> floatValues)
        {
            var binaryArray = new byte[floatValues.Count * 4];

            Buffer.BlockCopy(floatValues.ToArray(), 0, binaryArray, 0, binaryArray.Length);

            return binaryArray;
        }

        #endregion
    }

    #endregion

    #region Binary Bitmap Class

    public class BinaryBitmapData
    {
        #region Constructors

        public BinaryBitmapData(byte[] redData, byte[] greenData, byte[] blueData)
        {
            RedData = redData;
            GreenData = greenData;
            BlueData = blueData;
            SignType = TrafficSignType.Unclassified;
        }

        public BinaryBitmapData(byte[] redData, byte[] greenData, byte[] blueData, TrafficSignType signType)
        {
            RedData = redData;
            GreenData = greenData;
            BlueData = blueData;
            SignType = signType;
        }

        #endregion

        #region Properties

        public TrafficSignType SignType { get; set; }
        public byte[] RedData { get; set; }
        public byte[] GreenData { get; set; }
        public byte[] BlueData { get; set; }

        #endregion

        #region Methods

        public FloatBitmapData? ConvertToFloat()
        {
            var redFloat = ToFloatList(RedData);
            var greenFloat = ToFloatList(GreenData);
            var blueFloat = ToFloatList(BlueData);

            var floatBitmap = new FloatBitmapData(redFloat, greenFloat, blueFloat);

            return floatBitmap;
        }

        private List<float> ToFloatList(byte[] binaryValues)
        {
            var floatArray = new float[binaryValues.Length / 4];

            Buffer.BlockCopy(binaryValues, 0, floatArray, 0, floatArray.Length);

            return floatArray.ToList<float>();
        }

        #endregion
    }

    #endregion
}