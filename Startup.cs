// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.22.0


using bot.Dialogs;
using EchoBot1.Dialogs;
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
using SendGrid;

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
            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
      
      
         
            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<DatabaseService>();
            services.AddSingleton<InvoiceService>();
            services.AddSingleton<EmailService>();
            services.AddSingleton<SystemService>();
            services.AddSingleton<OrderService>();
            services.AddTransient<InvoiceActions>();
            // Configuração do UserProfileService
            services.AddSingleton<UserProfileService>();
            services.AddSingleton<BotState, ConversationState>();
            services.AddSingleton<BotState, UserState>();
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddTransient<KnowledgeBase>();
            services.AddSingleton<IStorageHelper,StorageHelper>();
          
            services.AddSingleton<UserProfile>();
            // Configuração do UserState e ConversationState
   
            // Configuração do MemoryStorage
            services.AddSingleton<IStorage, MemoryStorage>();
            services.AddTransient<EmailDialog>(provider =>
            {
                return new EmailDialog("your-string-value");
            });


            // Adicionar diálogos
            services.AddTransient<IssueResolutionDialog>();
            services.AddTransient<OrderDialog>();
            services.AddTransient<SupportDialog>();
          
            services.AddTransient<InvoiceDialog>();
            services.AddTransient<PersonalDataDialog>();
            services.AddTransient<MainMenuDialog>();

            services.AddSingleton<ISendGridClient>(provider =>
            {
                var apiKey = Configuration["SendGrid:ApiKey"];
                return new SendGridClient(apiKey);
            });

        
            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, Bots.EchoBot>();
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
