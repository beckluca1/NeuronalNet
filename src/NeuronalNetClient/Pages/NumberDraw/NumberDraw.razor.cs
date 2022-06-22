using Google.Protobuf;
using NeuronalNetClient.Proto;
using NeuronalNetClient.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NeuronalNetClient.Pages.NumberDraw
{
    public partial class NumberDraw
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        #region Overrides

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        #endregion

        #region Methods

        private void OnNumberButtonClicked(int number)
        {
            return;
        }

        private async Task UploadGeneratedBitmaps()
        {
            var uploader = new GrpcUploader("http://localhost:5018");

            var bitmaps = new List<BitmapData>()
            {
                new BitmapData() { Number = 1, Bitmap = ByteString.CopyFrom(new byte[] {0,1,0}) },
                new BitmapData() { Number = 2, Bitmap = ByteString.CopyFrom(new byte[] {1,0,1}) }
            };

            await uploader.UploadBitmaps(bitmaps);
        }

        
        private async Task CallJSFunction(String function, params Object[] args)
        { 
            await JSRuntime.InvokeVoidAsync(function, args);
        }

        #endregion
    }
}