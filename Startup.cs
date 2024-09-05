using EchoBot1;
using EchoBot1.Bots;

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
using System.Threading.Tasks;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configure settings from appsettings.json
        services.AddSingleton(Configuration);

        // Use ConfigurationBotFrameworkAuthentication as the implementation for BotFrameworkAuthentication
        services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>(sp =>
        {        
            return new ConfigurationBotFrameworkAuthentication(Configuration);
        });

        // Add bot adapter with error handling enabled
        services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
        // Register StorageHelper
        services.AddSingleton<IStorageHelper, StorageHelper>();

        // Add state management
        services.AddSingleton<IStorage, MemoryStorage>();
        services.AddSingleton<UserState>();

        services.AddSingleton<ConversationState>();


        // Register KnowledgeBase
        services.AddSingleton<KnowledgeBase>(provider =>
        {
            var knowledgeBase = new KnowledgeBase();    
            var knowledgeBasePath = Configuration["KnowledgeBasePath:path"];
            knowledgeBase.LoadResponses(knowledgeBasePath);
            return knowledgeBase;
        });


        // Add controllers and JSON support
        services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
        });

        // Register dialogs
        services.AddSingleton<PersonalDataDialog>();
        services.AddSingleton<LearningModeDialog>();   
        services.AddSingleton<MainDialog>();
        
        // Register the bot
        services.AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>();

        // Initialize the storage tables at startup


        var serviceProvider = services.BuildServiceProvider();
        var storageHelper = serviceProvider.GetRequiredService<IStorageHelper>();
        Task.Run(async () => await storageHelper.CreateTablesIfNotExistsAsync()).Wait();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseWebSockets();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
