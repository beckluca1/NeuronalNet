using Google.Protobuf;
using NeuronalNetClient.Proto;

using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NeuronalNetClient.Pages.Dashboard
{
    public partial class Dashboard
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        private Uploader.UploaderClient GrpcUploader { get; set; } = default!;

        private NumberOfSigns CurrentData { get; set; } = default!;

        #region Overrides

        protected override async void OnInitialized()
        {
            await GetNumberOfSigns();
        }

        #endregion

        #region Methods

        private async Task GetNumberOfSigns()
        {
            CurrentData = await GrpcUploader.GetSignTypeDataAsync(new Null());
        }

        #endregion
    }
}