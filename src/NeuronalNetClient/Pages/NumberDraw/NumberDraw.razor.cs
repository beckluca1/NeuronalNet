using Google.Protobuf;
using NeuronalNetClient.Proto;

using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NeuronalNetClient.Pages.NumberDraw
{
    public partial class NumberDraw
    {
        const int BinaryDataLength = 46 * 46;

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

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
            await GrpcUploader.SendTrafficSignAsync(trafficSign);
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
            Console.WriteLine("Got Data");
            for(int i=2;i<data[0]*data[0]+2;i++) 
            {
                Console.WriteLine(data[i*3]+" "+data[i*3+1]+" "+data[i*3+2]);
            }
        }

        #endregion
    }
}