using NeuronalNetServer.Services;

namespace NeuronalNetServer.Helpers
{
    public static class ServiceInitialization
    {
        public static void StartGrpcService()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddGrpc();

            var app = builder.Build();

            app.MapGrpcService<UploadService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }

        public static void StartNeuralNetService()
        {
            //TODO: add processes of NeuralNet project
        }
    }
}