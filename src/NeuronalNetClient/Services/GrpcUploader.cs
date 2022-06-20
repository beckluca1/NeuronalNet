using Grpc.Net.Client;
using NeuronalNetClient.Proto;
using static NeuronalNetClient.Proto.Uploader;
using Grpc.Net.Client.Web;

namespace NeuronalNetClient.Services
{
    public class GrpcUploader
    {
        #region Fields
        private GrpcChannel _grpcChannel;

        #endregion

        #region Constructor

        public GrpcUploader(string address)
        {
            //TODO: add TLS
            var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());
            _grpcChannel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpHandler = httpHandler} );
        }

        #endregion

        #region Methods

        public async Task UploadBitmaps(List<BitmapData> bitmaps)
        {
            UploaderClient client = new UploaderClient(_grpcChannel);

            using (var streamingCall = client.SendBitmapData())
            {
                foreach (BitmapData bitmap in bitmaps)
                {
                    await streamingCall.RequestStream.WriteAsync(bitmap);
                }
                await streamingCall.RequestStream.CompleteAsync();
            }
        }

        #endregion
    }
}