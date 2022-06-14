using NeuronalNetClient;
using Grpc.Net.Client;
using NeuronalNetClient.Proto;
using Google.Protobuf;
using static NeuronalNetClient.Proto.Uploader;

namespace NeuronalNetClient.Services
{
    public class GrpcUploader
    {
        private GrpcChannel _grpcChannel;

        #region Constructor

        public GrpcUploader(string address)
        {
            //TODO: add TLS
            _grpcChannel = GrpcChannel.ForAddress(address);
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