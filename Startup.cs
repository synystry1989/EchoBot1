using EchoBot1.Bots;
using EchoBot1.Dialogs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EchoBot1.Services;
using EchoBot1.Servicos;
using Microsoft.Extensions.Logging;
using bot.Dialogs;
using static bot.Dialogs.CancelOrderDialog;


namespace EchoBot1
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            // Bot Framework Authentication
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Adapter
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // State storage
            services.AddSingleton<IStorage, MemoryStorage>();
            services.AddSingleton<UserState>();
            services.AddSingleton<ConversationState>();

            // Services
            services.AddSingleton<UserProfileService>();
            services.AddSingleton<KnowledgeBase>();
            services.AddSingleton<OpenAIService>();
            services.AddSingleton<InvoiceActions>();
            services.AddSingleton<OrderService>();
            services.AddSingleton<IStorageHelper, StorageHelper>();
            services.AddSingleton<SystemService>();
            services.AddSingleton<EmailService>();

            // Dialogs
            services.AddSingleton<MainDialog>();
            services.AddSingleton<WelcomeBot>();
            services.AddSingleton<LearningDialog>();
            services.AddSingleton<EmpresarialDialog>();
            services.AddSingleton<PersonalDataDialog>();
            services.AddSingleton<EmailDialog>();
            services.AddSingleton<InvoiceDialog>();
            services.AddSingleton<SupportDialog>();
            services.AddSingleton<OrderDialog>();
            services.AddSingleton<CancelOrderDialog>();
            services.AddSingleton<ModifyOrderDialog>();
            services.AddSingleton<IssueResolutionDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogBot<MainDialog>>(sp =>
            {
                var conversationState = sp.GetRequiredService<ConversationState>();
                var userState = sp.GetRequiredService<UserState>();
                var mainDialog = sp.GetRequiredService<MainDialog>();
                var logger = sp.GetRequiredService<ILogger<DialogBot<MainDialog>>>();

                return new DialogBot<MainDialog>(conversationState, userState, mainDialog, logger);
            });
        }

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
        }
    }
}
