using Grpc.Core;
using NeuronalNetServer.Proto;

namespace NeuronalNetServer.Services
{
    public class UploadService : Uploader.UploaderBase
    {
        #region Fields
        private readonly ILogger<UploadService> _logger;

        #endregion

        #region Constructor

        public UploadService(ILogger<UploadService> logger)
        {
            _logger = logger;
        }

        #endregion

        #region Methods

        public override async Task<SuccessReply> SendBitmapData(IAsyncStreamReader<BitmapData> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var bitmap = requestStream.Current;

                //TODO: process BitmapData
            }

            return new SuccessReply()
            {
                Success = true
            };
        }

        public override Task<SuccessReply> SendTrafficSign(TrafficSign request, ServerCallContext context)
        {
            //TODO: process traffic sign
            return base.SendTrafficSign(request, context);
        }

        public override async Task<SuccessReply> SendMultipleTrafficSigns(IAsyncStreamReader<TrafficSign> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var trafficSignData = requestStream.Current;

                //TODO: process traffic sign
            }

            return new SuccessReply()
            {
                Success = true
            };

        }

        #endregion
    }
}
