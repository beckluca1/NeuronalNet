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

        private async Task UploadBitmaps()
        {
            var bitmaps = ConvertGeneratedBitmaps();

            using (var streamingCall = GrpcUploader.SendBitmapData())
            {
                foreach (BitmapData bitmap in bitmaps)
                {
                    await streamingCall.RequestStream.WriteAsync(bitmap);
                }
                await streamingCall.RequestStream.CompleteAsync();
            }
        }

        private void OnNumberButtonClicked(int number)
        {
            return;
        }

        private List<BitmapData> ConvertGeneratedBitmaps()
        {
            //TODO: get generated bitmaps and convert them to BitmapData objects

            var bitmaps = new List<BitmapData>()
            {
                new BitmapData() { Number = 1, Bitmap = ByteString.CopyFrom(new byte[] {0,1,0}) },
                new BitmapData() { Number = 2, Bitmap = ByteString.CopyFrom(new byte[] {1,0,1}) }
            };

            return bitmaps;
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
            for(int i=1;i<data[0]*data[0]+2;i++) 
            {
                Console.WriteLine(data[i*3]+" "+data[i*3+1]+" "+data[i*3+2]);
            }
        }

        #endregion
    }
}