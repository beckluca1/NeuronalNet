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

            for(int i=0;i<15000000;i++)
            {

                if(i>0&&i%10==0)
                    calculator.Improve();
                
                calculator.Calculate(type);


                type = (type+1) % 5;
                //Console.WriteLine(type);

                Thread.Sleep(10);
            }
        }
    }
}