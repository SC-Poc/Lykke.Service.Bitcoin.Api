using System;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables.Entity.Metamodel;
using Lykke.AzureStorage.Tables.Entity.Metamodel.Providers;
using Lykke.Logs.Loggers.LykkeSlack;
using Lykke.Sdk;
using Lykke.Service.Bitcoin.Api.Middleware;
using Lykke.Service.Bitcoin.Api.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Service.Bitcoin.Api
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "Bitcoin.Api API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                EntityMetamodel.Configure(new AnnotationsBasedMetamodelProvider());

                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "BitcoinApiLog";
                    logs.AzureTableConnectionStringResolver = settings => settings.Bitcoin.Db.LogsConnString;

                    logs.Extended = extendedLogs =>
                    {
                        extendedLogs.AddAdditionalSlackChannel("BlockChainIntegration");
                        extendedLogs.AddAdditionalSlackChannel("BlockChainIntegrationImportantMessages",
                            channelOptions => { channelOptions.MinLogLevel = LogLevel.Warning; });
                    };
                };               
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
                options.WithMiddleware = builder => builder.UseCustomErrorHandligMiddleware();
            });
        }
    }
}
