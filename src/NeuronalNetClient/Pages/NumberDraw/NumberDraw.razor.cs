using Google.Protobuf;
using NeuronalNetClient.Proto;

using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NeuronalNetClient.Pages.NumberDraw
{
    public partial class NumberDraw
    {
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

        private void UploadTrafficSign()
        {
            var trafficSign = GenerateRandomTraffigSign();
            GrpcUploader.SendTrafficSign(trafficSign);
        }

        private void OnNumberButtonClicked(int number)
        {
            return;
        }

        private TrafficSign GenerateRandomTraffigSign()
        {
            var random = new Random();

            byte[] red = new byte[46 * 4];
            byte[] green = new byte[46 * 4];
            byte[] blue = new byte[46 * 4];

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