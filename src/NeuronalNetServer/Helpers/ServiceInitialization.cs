using NeuronalNetServer.Services;
using NeuronalNetServer.Proto;

namespace NeuronalNetServer.Helpers
{
    public static class ServiceInitialization
    {
        public static void StartGrpcService()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddCors(o => o.AddPolicy("CorePolicy", builder =>
            {
                builder.AllowAnyHeader();
                builder.AllowAnyOrigin();
                builder.AllowAnyMethod();
            }));

            builder.Services.AddGrpc();
            builder.Services.AddScoped<DatabaseService>();

            var app = builder.Build();

            app.UseCors("CorePolicy");

            app.UseRouting();
            app.UseGrpcWeb();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<UploadService>().EnableGrpcWeb();
            });

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