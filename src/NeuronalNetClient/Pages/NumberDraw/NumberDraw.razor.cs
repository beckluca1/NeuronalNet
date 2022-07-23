using Google.Protobuf;
using NeuronalNetClient.Proto;

using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using NeuralNet;

namespace NeuronalNetClient.Pages.NumberDraw
{
    public partial class NumberDraw
    {
        private static ConvolutionalNet currentNet = new ConvolutionalNet();
        private static TrafficSign toBeUploadedSign = new TrafficSign();

        const int BinaryDataLength = 46 * 46;

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

        private async Task UploadTrafficSign()
        {
            var trafficSign = GenerateRandomTraffigSign();
            await GrpcUploader.SendTrafficSignAsync(toBeUploadedSign);
        }

        private async Task<NeuralNetData> GetLatestNeuralNet()
        {
            NeuralNetData latestNeuralNet = await GrpcUploader.GetNeuralNetDataAsync(new Null());

            return latestNeuralNet;
        }

        private async Task UploadMultipleTrafficSigns()
        {
            var trafficSignList = new List<TrafficSign>();
            trafficSignList.Add(GenerateRandomTraffigSign());

            using (var streamingCall = GrpcUploader.SendMultipleTrafficSigns())
            {
                foreach (TrafficSign sign in trafficSignList)
                {
                    await streamingCall.RequestStream.WriteAsync(sign);
                }
                await streamingCall.RequestStream.CompleteAsync();
            }
        }

        private void OnNumberButtonClicked(int number)
        {
            return;
        }

        private TrafficSign GenerateRandomTraffigSign()
        {
            var random = new Random();

            byte[] red = new byte[BinaryDataLength];
            byte[] green = new byte[BinaryDataLength];
            byte[] blue = new byte[BinaryDataLength];

            random.NextBytes(red);
            random.NextBytes(green);
            random.NextBytes(blue);

            return new TrafficSign()
            {
                SignType = SignType.Unclassified,
                Red = Google.Protobuf.ByteString.CopyFrom(red),
                Green = Google.Protobuf.ByteString.CopyFrom(green),
                Blue = Google.Protobuf.ByteString.CopyFrom(blue)
            };  
        }
        
        private async Task CallJSFunction(String function, params Object[] args)
        { 
            await JSRuntime.InvokeVoidAsync(function, args);
        }

        [JSInvokable]
        public static void GetImageData(int[] data)
        {
            int number = data[1];
            List<byte> redData = new List<byte>();
            List<byte> greenData = new List<byte>();
            List<byte> blueData = new List<byte>();

            Console.WriteLine("Got Data"+data[0]);
            for(int i=0;i<data[0]*data[0];i++) 
            {
                redData.Add((byte)data[i*3+2]);
                greenData.Add((byte)data[i*3+3]);
                blueData.Add((byte)data[i*3+4]);
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
        public static void GetNetData(int[] data)
        {
            List<byte> redData = new List<byte>();
            List<byte> greenData = new List<byte>();
            List<byte> blueData = new List<byte>();

            Console.WriteLine("Got Data"+data[0]);
            for(int i=0;i<data[0]*data[0];i++) 
            {
                redData.Add((byte)data[i*3+1]);
                greenData.Add((byte)data[i*3+2]);
                blueData.Add((byte)data[i*3+3]);
            }

            currentNet.SetInput(redData.ToArray(), greenData.ToArray(), blueData.ToArray());
            currentNet.Update();
        }

        public async void UpdateJSNet()
        {
            currentNet = NetSaveStateHandler.readFromSaveState((await GetLatestNeuralNet()).NetData.ToByteArray());
            await CallJSFunction("updateNet",currentNet.GetOutput().ToArray());
        }

        #endregion
    }
}