using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Common.Log;
using Lykke.Job.Bitcoin.Modules;
using Lykke.Job.Bitcoin.Settings;
using Lykke.JobTriggers.Triggers;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeSlack;
using Lykke.MonitoringServiceApiCaller;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lykke.Job.Bitcoin
{
    [PublicAPI]
    public class Startup
    {
        private const string ApiVersion = "v1";
        private const string ApiName = "Bitcoin Job";
        private IHealthNotifier _healthNotifier;
        private ILog _log;
        private string _monitoringServiceUrl;

        private TriggerHost _triggerHost;
        private Task _triggerHostTask;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;
        }

        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.Converters.Add(new StringEnumConverter());
                        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    });

                services.AddSwaggerGen(options => { options.DefaultLykkeConfiguration(ApiVersion, ApiName); });

                var settingsManager = Configuration.LoadSettings<AppSettings>(options =>
                {
                    options.SetConnString(x => x.SlackNotifications.AzureQueue.ConnectionString);
                    options.SetQueueName(x => x.SlackNotifications.AzureQueue.QueueName);
                    options.SenderName = $"{AppEnvironment.Name} {AppEnvironment.Version}";
                });

                var appSettings = settingsManager.CurrentValue;
                if (appSettings.MonitoringServiceClient != null)
                    _monitoringServiceUrl = appSettings.MonitoringServiceClient.MonitoringServiceUrl;
                services.AddLykkeLogging(
                    settingsManager.ConnectionString(s => s.Bitcoin.Db.LogsConnString),
                    "BitcoinJobLog",
                    appSettings.SlackNotifications.AzureQueue.ConnectionString,
                    appSettings.SlackNotifications.AzureQueue.QueueName,
                    logging =>
                    {
                        logging.AddAdditionalSlackChannel("BlockChainIntegration");
                        logging.AddAdditionalSlackChannel("BlockChainIntegrationImportantMessages",
                            options => { options.MinLogLevel = LogLevel.Warning; });
                    });

                var builder = new ContainerBuilder();
                builder.Populate(services);

                builder.RegisterModule(new BitcoinJobModule(settingsManager));
                builder.RegisterModule(new RepositoryModule(settingsManager));
                builder.RegisterModule(new ServiceModule(settingsManager));

                ApplicationContainer = builder.Build();

                var logFactory = ApplicationContainer.Resolve<ILogFactory>();
                _log = logFactory.CreateLog(this);
                _healthNotifier = ApplicationContainer.Resolve<IHealthNotifier>();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                if (_log == null)
                    Console.WriteLine(ex);
                else
                    _log.Critical(ex);
                throw;
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                    app.UseDeveloperExceptionPage();

                app.UseLykkeForwardedHeaders();
                app.UseLykkeMiddleware(ex => new ErrorResponse {ErrorMessage = "Technical problem"});

                app.UseMvc();
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint($"/swagger/{ApiVersion}/swagger.json", ApiVersion);
                });
                app.UseStaticFiles();

                appLifetime.ApplicationStarted.Register(() => StartApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopping.Register(() => StopApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopped.Register(CleanUp);
            }
            catch (Exception ex)
            {
                _log?.Critical(ex);
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Job not yet recieve and process IsAlive requests here
#if !DEBUG
                await Configuration.RegisterInMonitoringServiceAsync(_monitoringServiceUrl, _healthNotifier);
#endif
                _triggerHost = new TriggerHost(new AutofacServiceProvider(ApplicationContainer));

                _triggerHostTask = _triggerHost.Start();
                _healthNotifier.Notify("Started", Program.EnvInfo);
            }
            catch (Exception ex)
            {
                _log.Critical(ex);
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                _triggerHost?.Cancel();

                if (_triggerHostTask != null)
                    await _triggerHostTask;
            }
            catch (Exception ex)
            {
                _log?.Critical(ex);
                throw;
            }
        }

        private void CleanUp()
        {
            try
            {
                _healthNotifier?.Notify("Terminating", Program.EnvInfo);

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                _log?.Critical(ex);
                throw;
            }
        }
    }
}
