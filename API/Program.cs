using _UI;
using API.Extensions;
using Microsoft.Extensions.Options;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Options
            builder.Configuration.AddEntityConfiguration();

            builder.Services.Configure<ServerOptions>(
            builder.Configuration.GetSection("ServerOptions"));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            //using IHost host = builder.Build();

            ServerOptions options = app.Services.GetRequiredService<IOptions<ServerOptions>>().Value;
            Console.WriteLine($"InboundAddress={options.InboundAddress}");
            Console.WriteLine($"InboundPort={options.InboundPort}");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Urls.Add(options.InboundAddress + ":" + options.InboundPort);
            app.Run();
        }
    }
}
