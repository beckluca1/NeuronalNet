using NeuronalNetServer.Services;
using NeuronalNetServer.Proto;

namespace NeuronalNetServer.Helpers
{
    public static class ServiceInitialization
    {
        public static void StartGrpcService()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddGrpc();
            builder.Services.AddScoped<DatabaseService>();

            var app = builder.Build();

            app.MapGrpcService<UploadService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }

        public static void StartNeuralNetService()
        {
            Console.WriteLine("Init Neural Net");

            NeuralCalculator calculator = new NeuralCalculator();

            int type = 0;

            while(true)
            {
                type = type == 5 ? 0 : type+1;
                
                //calculator.Calculate(type);

                Thread.Sleep(100);
                Console.WriteLine("Calculate");
            }
        }
    }
}