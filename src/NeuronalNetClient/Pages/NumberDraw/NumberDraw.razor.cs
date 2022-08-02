using NeuronalNetClient.Proto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NeuralNet;

namespace NeuronalNetClient.Pages.NumberDraw
{
    public partial class NumberDraw
    {
        private static ConvolutionalNet currentCNN = new ConvolutionalNet();
        private static ProposalNeuralNet currentRPN = new ProposalNeuralNet();

        private static TrafficSign toBeUploadedSign = new TrafficSign();
        private static TrafficImage toBeUploadedImage = new TrafficImage();

        const int BinaryDataLength = 48 * 48;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        private Uploader.UploaderClient GrpcUploader { get; set; } = default!;

        #region Overrides

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        #endregion

        #region Methods

        private async Task Upload()
        {
            await GrpcUploader.SendTrafficSignAsync(toBeUploadedSign);

            await GrpcUploader.SendTrafficImageAsync(toBeUploadedImage);
        }

        private async Task<NeuralNetData> GetLatestCNN()
        {
            NeuralNetData latestNeuralNet = await GrpcUploader.GetCNNDataAsync(new Null());

            return latestNeuralNet;
        }

        private async Task<NeuralNetData> GetLatestRPN()
        {
            NeuralNetData latestNeuralNet = await GrpcUploader.GetRPNDataAsync(new Null());

            return latestNeuralNet;
        }

        private async Task CallJSFunction(String function, params Object[] args)
        {
            await JSRuntime.InvokeVoidAsync(function, args);
        }

        [JSInvokable]
        public static void UploadSignImageData(int[] data)
        {
            int number = data[1];
            List<byte> redData = new List<byte>();
            List<byte> greenData = new List<byte>();
            List<byte> blueData = new List<byte>();

            Console.WriteLine("Got Data" + data[0]);
            for (int i = 0; i < data[0] * data[0]; i++)
            {
                redData.Add((byte)data[i * 3 + 2]);
                greenData.Add((byte)data[i * 3 + 3]);
                blueData.Add((byte)data[i * 3 + 4]);
            }

            TrafficSign sign = new TrafficSign()
            {
                SignType = (SignType)number,
                Red = Google.Protobuf.ByteString.CopyFrom(redData.ToArray()),
                Green = Google.Protobuf.ByteString.CopyFrom(greenData.ToArray()),
                Blue = Google.Protobuf.ByteString.CopyFrom(blueData.ToArray())
            };

            toBeUploadedSign = sign;
        }

        [JSInvokable]
        public static void UploadTrafficImageData(int[] data)
        {
            int number = data[1];
            List<byte> redData = new List<byte>();
            List<byte> greenData = new List<byte>();
            List<byte> blueData = new List<byte>();
            List<byte> locationData = new List<byte>();

            Console.WriteLine("Got Data" + data[0]);
            for (int i = 0; i < data[0] * data[0]; i++)
            {
                redData.Add((byte)data[i * 3 + 2]);
                greenData.Add((byte)data[i * 3 + 3]);
                blueData.Add((byte)data[i * 3 + 4]);
            }

            for (int i = 0; i < number; i++)
            {
                locationData.Add((byte)data[i * 4 + 2 + data[0] * data[0] * 3]);
                locationData.Add((byte)data[i * 4 + 3 + data[0] * data[0] * 3]);
                locationData.Add((byte)data[i * 4 + 4 + data[0] * data[0] * 3]);
                locationData.Add((byte)data[i * 4 + 5 + data[0] * data[0] * 3]);
            }

            TrafficImage image = new TrafficImage()
            {
                SignCount = (uint)number,
                Red = Google.Protobuf.ByteString.CopyFrom(redData.ToArray()),
                Green = Google.Protobuf.ByteString.CopyFrom(greenData.ToArray()),
                Blue = Google.Protobuf.ByteString.CopyFrom(blueData.ToArray()),
                Location = Google.Protobuf.ByteString.CopyFrom(locationData.ToArray())
            };

            toBeUploadedImage = image;
        }

        [JSInvokable]
        public static void UpdateCNNImage(int[] data)
        {
            List<byte> redData = new List<byte>();
            List<byte> greenData = new List<byte>();
            List<byte> blueData = new List<byte>();

            Console.WriteLine("Got Data" + data[0]);
            for (int i = 0; i < data[0] * data[0]; i++)
            {
                redData.Add((byte)data[i * 3 + 1]);
                greenData.Add((byte)data[i * 3 + 2]);
                blueData.Add((byte)data[i * 3 + 3]);
            }

            currentCNN.SetInput(redData.ToArray(), greenData.ToArray(), blueData.ToArray());
            currentCNN.Update();
        }

        [JSInvokable]
        public static void UpdateRPNImage(int[] data)
        {
            List<byte> redData = new List<byte>();
            List<byte> greenData = new List<byte>();
            List<byte> blueData = new List<byte>();

            Console.WriteLine("Got Data" + data[0]);
            for (int i = 0; i < data[0] * data[0]; i++)
            {
                redData.Add((byte)data[i * 3 + 1]);
                greenData.Add((byte)data[i * 3 + 2]);
                blueData.Add((byte)data[i * 3 + 3]);
            }

            currentRPN.SetInput(redData.ToArray(), greenData.ToArray(), blueData.ToArray());
            currentRPN.Update();
        }

        public async void Init()
        {
            await CallJSFunction("init");
            GetBestNet();
        }

        public async void GetBestNet()
        {
            currentCNN = NetSaveStateHandler.readFromSaveStateCNN((await GetLatestCNN()).NetData.ToByteArray());
            currentRPN = NetSaveStateHandler.readFromSaveStateRPN((await GetLatestRPN()).NetData.ToByteArray());
        }

        public async void GetNetOutput()
        {
            await CallJSFunction("callCNNUpdate");
            await CallJSFunction("callRPNUpdate");

            await CallJSFunction("updateCNN", currentCNN.GetOutput().ToArray());

            Rectangle bestRectangle = currentRPN.presesntBest();
            float[] output = new float[]
            {
                bestRectangle.x-bestRectangle.width/2,
                bestRectangle.y-bestRectangle.height/2,
                bestRectangle.x+bestRectangle.width/2,
                bestRectangle.y+bestRectangle.height/2
            };

            await CallJSFunction("updateRPN", output);
        }

        #endregion
    }
}