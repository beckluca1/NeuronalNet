using NeuronalNetServer.Services;
using NeuronalNetServer.Proto;

using NeuralNet;

namespace NeuronalNetServer.Helpers
{
    public static class ServiceInitialization
    {
        static NeuralCalculator calculator = new NeuralCalculator();
        static bool shutDown = false;

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

            calculator.InitNet();

            int i=0;
            while(true)
            {
                if(i>0&&i%Global.BATCH_SIZE==0)
                {
                    calculator.ImproveCNN();
                    calculator.ImproveRPN();
                }

                if(i>0&&i%1000==0)
                {
                    calculator.uploadCurrentNet();
                    Console.WriteLine("Updated net");
                }
                            
                int type = new Random().Next() % 5;
                calculator.CalculateCNN(type);
                calculator.CalculateRPN();

                Thread.Sleep(1);
                i++;
            }
        }
    }
}