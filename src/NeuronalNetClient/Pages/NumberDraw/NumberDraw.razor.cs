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

        private async Task UploadTrafficSign()
        {
            var trafficSign = GenerateRandomTraffigSign();
            await GrpcUploader.SendTrafficSignAsync(toBeUploadedSign);
        }

        private async Task UploadTrafficImage()
        {
            await GrpcUploader.SendTrafficImageAsync(toBeUploadedImage);
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
        public static void UploadSignImageData(int[] data)
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
        public static void UploadTrafficImageData(int[] data)
        {
            int number = data[1];
            List<byte> redData = new List<byte>();
            List<byte> greenData = new List<byte>();
            List<byte> blueData = new List<byte>();
            List<byte> locationData = new List<byte>();

            Console.WriteLine("Got Data"+data[0]);
            for(int i=0;i<data[0]*data[0];i++) 
            {
                redData.Add((byte)data[i*3+2]);
                greenData.Add((byte)data[i*3+3]);
                blueData.Add((byte)data[i*3+4]);
            }

            for(int i=0;i<number;i++) 
            {
                locationData.Add((byte)data[i*4+2+data[0]*data[0]*3]);
                locationData.Add((byte)data[i*4+3+data[0]*data[0]*3]);
                locationData.Add((byte)data[i*4+4+data[0]*data[0]*3]);
                locationData.Add((byte)data[i*4+5+data[0]*data[0]*3]);
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
        public static void UpdateNetImage(int[] data)
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

        public async void Init()
        {
            await CallJSFunction("init");
            GetBestNet();
        }

        public async void GetBestNet()
        {
            currentNet = NetSaveStateHandler.readFromSaveState((await GetLatestNeuralNet()).NetData.ToByteArray());
        }
        

        public async void GetNetOutput()
        {
            await CallJSFunction("callNetUpdate");

            await CallJSFunction("updateNet",currentNet.GetOutput().ToArray());
        }

        #endregion
    }
}