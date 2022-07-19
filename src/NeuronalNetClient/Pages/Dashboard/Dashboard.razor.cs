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
        private Uploader.UploaderClient GrpcUploader { get; set; } = default!;

        private NumberOfSigns _currentData = default!;
        private NumberOfSigns CurrentData
        {
            get
            {
                return _currentData;
            }
            set
            {
                Total = CalculateTotal();
                _currentData = value;
            }
        }

        private int Total { get; set; }

        #region Overrides

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        #endregion

        #region Methods

        private async Task GetNumberOfSigns()
        {
            CurrentData = await GrpcUploader.GetSignTypeDataAsync(new Null());
        }

        private int CalculateTotal()
        {
            if (CurrentData == null)
                return 0;

            int total = CurrentData.Stop
                        + CurrentData.GiveWay
                        + CurrentData.PriorityRoad
                        + CurrentData.ThirtySpeedLimit
                        + CurrentData.FiftySpeedLimit
                        + CurrentData.Unclassified;

            return total;
        }

        #endregion
    }
}