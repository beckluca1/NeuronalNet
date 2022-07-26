using Grpc.Core;
using NeuronalNetServer.Helpers;
using NeuronalNetServer.Proto;

namespace NeuronalNetServer.Services
{
    public class UploadService : Uploader.UploaderBase
    {
        #region Fields
        private Credentials _credentials = default!;
        private readonly ILogger<UploadService> _logger;
        private readonly DatabaseService _dbService;

        #endregion

        #region Constructor

        public UploadService(ILogger<UploadService> logger, DatabaseService dbService)
        {
            _logger = logger;
            _dbService = dbService;

            GetConfiguration();
            InitiliazeServices();

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
            _dbService.InsertTrafficSign(request);

            var reply = new SuccessReply() { Success = true };

            return Task.FromResult(reply);
        }

        public override async Task<SuccessReply> SendMultipleTrafficSigns(IAsyncStreamReader<TrafficSign> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var trafficSignData = requestStream.Current;

                _dbService.InsertTrafficSign(trafficSignData);
            }

            return new SuccessReply()
            {
                Success = true
            };
        }

        public override Task<NumberOfSigns> GetSignTypeData(Null request, ServerCallContext context)
        {
            var numberOfSigns = _dbService.GetNumberOfSigns();

            return Task.FromResult(numberOfSigns);
        }

        public override Task<SuccessReply> SendTrafficImage(TrafficImage request, ServerCallContext context)
        {
            _dbService.InsertTrafficImage(request);

            var reply = new SuccessReply() { Success = true };

            return Task.FromResult(reply);
        }

        public override Task<Proto.NeuralNetData> GetNeuralNetData(Null request, ServerCallContext context)
        {
            var neuralNetData = _dbService.GetLatestNeuralNet();

            return Task.FromResult(neuralNetData);
        }
        private void InitiliazeServices()
        {
            _dbService.Initialize(_credentials!.DbConnectionString!);
        }

        private void GetConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>(optional: true)
                .AddEnvironmentVariables()
                .Build();
            var neuroSection = config.GetSection("NEURO");

            _credentials = new Credentials
            {
                DbConnectionString = neuroSection["DB_CONNECTION_STRING"],
            };
        }

        #endregion
    }
}
