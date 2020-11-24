using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TwitterApi.Models;
using TwitterApi.Services;
using TwitterStream.Services;

namespace TwitterStream
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
             
            var twitterKeys = new TwitterSettings();
            Configuration.Bind("Twitter", twitterKeys);
            services.AddSingleton(twitterKeys); 
            services.AddSingleton<ILogger, ConsoleLogService>();
            services.AddSingleton<ITwitterApiService, TwitterApiService>();
            services.AddSingleton<IEmojiService, EmojiService>();
            services.AddSingleton<ITwitterService, TwitterService>();
            services.AddSingleton<ITwitterDatabase, TwitterDatabase>(); 
            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "Twitter  Statistics", Version = "v1" }); 
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Twitter Feed Statistics");
            });
        }
    }
}
