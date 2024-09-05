// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.22.0

using EchoBot1.Bots;
using EchoBot1.Dialogos;
using EchoBot1.Modelos;
using EchoBot1.Servicos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace EchoBot1
{
    public class Startup
    { 
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


          


            services.AddSingleton<KnowledgeBase>(provider =>
            {
                var knowledgeBase = new KnowledgeBase();
                var config = provider.GetRequiredService<IConfiguration>();
                var knowledgeBasePath = config["KnowledgeBasePath:path"];
                knowledgeBase.LoadResponses(knowledgeBasePath);
                return knowledgeBase;
            });

                services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.AddTransient<KnowledgeBase>();
            services.AddSingleton<IStorageHelper,StorageHelper>();
            services.AddSingleton<MainDialog>();

            var serviceProvider = services.BuildServiceProvider();
            var storageHelper = serviceProvider.GetRequiredService<IStorageHelper>();
            Task.Run(async () => await storageHelper.CreateTablesIfNotExistsAsync()).Wait();
            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
