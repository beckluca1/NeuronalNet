using Grpc.Core;
using NeuronalNetServer.Proto;

namespace NeuronalNetServer.Services;

public class UploadService : Uploader.UploaderBase
{
    private readonly ILogger<UploadService> _logger;
    public UploadService(ILogger<UploadService> logger)
    {
        _logger = logger;
    }

    public override Task<SuccessReply> SendBitmapData(IAsyncStreamReader<BitmapData> requestStream, ServerCallContext context)
    {
        //TODO: do something with requestStream

        return Task.FromResult(new SuccessReply
        {
            Success = true
        });
    }

}
