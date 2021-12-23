using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Rose.Infra.Data.ChangeInterceptors.EntityChageInterceptorItems;
using Rose.Infra.Events.Outbox;
using Rose.Infra.Events.PoolingPublisher;
using Rose.Infra.Tools.Caching.Microsoft;
using Rose.Infra.Tools.OM.AutoMapper.DipendencyInjections;
using Rose.Messaging.IdempotentConsumers;
using Rose.Utilities;
using Rose.Utilities.Configurations;
using Rose.Utilities.Services.Chaching;
using Rose.Utilities.Services.Localizations;
using Rose.Utilities.Services.Logger;
using Rose.Utilities.Services.MessageBus;
using Rose.Utilities.Services.Serializers;
using Rose.Utilities.Services.Users;
using System.Reflection;

namespace Rose.EndPoints.Web.StartupExtentions
{
    public static class AddRoseServicesExtentions
    {
        public static IServiceCollection AddRoseServices(
            this IServiceCollection services,
            IEnumerable<Assembly> assembliesForSearch)
        {
            services.AddCaching();
            services.AddSession();
            services.AddLogging();
            services.AddJsonSerializer(assembliesForSearch);
            services.AddExcelSerializer(assembliesForSearch);
            services.AddObjectMapper(assembliesForSearch);
            services.AddUserInfoService(assembliesForSearch);
            services.AddTranslator(assembliesForSearch);
            services.AddMessageBus(assembliesForSearch);
            services.AddPoolingPublisher(assembliesForSearch);
            services.AddTransient<RoseServices>();
            services.AddEntityChangeInterception(assembliesForSearch);
            return services;
        }

        private static IServiceCollection AddCaching(this IServiceCollection services)
        {
            var _RoseConfigurations = services.BuildServiceProvider().GetService<RoseConfigurationOptions>();
            if (_RoseConfigurations?.Caching?.Enable == true)
            {
                if (_RoseConfigurations.Caching.Provider == CacheProvider.MemoryCache)
                {
                    services.AddTransient<ICacheAdapter, InMemoryCacheAdapter>();
                }
                else
                {
                    services.AddTransient<ICacheAdapter, DistributedCacheAdapter>();
                }

                switch (_RoseConfigurations.Caching.Provider)
                {
                    case CacheProvider.DistributedSqlServerCache:
                        services.AddDistributedSqlServerCache(options =>
                        {
                            options.ConnectionString = _RoseConfigurations.Caching.DistributedSqlServerCache.ConnectionString;
                            options.SchemaName = _RoseConfigurations.Caching.DistributedSqlServerCache.SchemaName;
                            options.TableName = _RoseConfigurations.Caching.DistributedSqlServerCache.TableName;
                        });
                        break;
                    case CacheProvider.StackExchangeRedisCache:
                        services.AddStackExchangeRedisCache(options =>
                        {
                            options.Configuration = _RoseConfigurations.Caching.StackExchangeRedisCache.Configuration;
                            options.InstanceName = _RoseConfigurations.Caching.StackExchangeRedisCache.SampleInstance;
                        });
                        break;
                    case CacheProvider.NCacheDistributedCache:
                        throw new NotSupportedException("NCache Not Supporting yet");
                    default:
                        services.AddMemoryCache();
                        break;
                }
            }
            else
            {
                services.AddScoped<ICacheAdapter, FakeCacheAdapter>();
            }
            return services;
        }
        private static IServiceCollection AddSession(this IServiceCollection services)
        {
            var _RoseConfigurations = services.BuildServiceProvider().GetService<RoseConfigurationOptions>();
            if (_RoseConfigurations?.Session?.Enable == true)
            {
                var eveSessionCookie = _RoseConfigurations.Session.Cookie;
                CookieBuilder cookieBuilder = new();
                cookieBuilder.Name = eveSessionCookie.Name;
                cookieBuilder.Domain = eveSessionCookie.Domain;
                cookieBuilder.Expiration = eveSessionCookie.Expiration;
                cookieBuilder.HttpOnly = eveSessionCookie.HttpOnly;
                cookieBuilder.IsEssential = eveSessionCookie.IsEssential;
                cookieBuilder.MaxAge = eveSessionCookie.MaxAge;
                cookieBuilder.Path = eveSessionCookie.Path;
                cookieBuilder.SameSite = Enum.Parse<SameSiteMode>(eveSessionCookie.SameSite.ToString());
                cookieBuilder.SecurePolicy = Enum.Parse<CookieSecurePolicy>(eveSessionCookie.SecurePolicy.ToString());

                services.AddSession(options =>
                {
                    options.Cookie = cookieBuilder;
                    options.IdleTimeout = _RoseConfigurations.Session.IdleTimeout;
                    options.IOTimeout = _RoseConfigurations.Session.IOTimeout;
                });
            }
            return services;
        }
        private static IServiceCollection AddLogging(this IServiceCollection services)
        {
            return services.AddScoped<IScopeInformation, ScopeInformation>();
        }

        public static IServiceCollection AddJsonSerializer(this IServiceCollection services, IEnumerable<Assembly> assembliesForSearch)
        {
            var _RoseConfigurations = services.BuildServiceProvider().GetService<RoseConfigurationOptions>();
            services.Scan(s => s.FromAssemblies(assembliesForSearch)
                .AddClasses(c => c.Where(type => type.Name == _RoseConfigurations.JsonSerializerTypeName && typeof(IJsonSerializer).IsAssignableFrom(type)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());
            return services;
        }

        public static IServiceCollection AddExcelSerializer(this IServiceCollection services, IEnumerable<Assembly> assembliesForSearch)
        {
            var _RoseConfigurations = services.BuildServiceProvider().GetService<RoseConfigurationOptions>();
            services.Scan(s => s.FromAssemblies(assembliesForSearch)
                .AddClasses(classes => classes.Where(type => type.Name == _RoseConfigurations.ExcelSerializerTypeName && typeof(IExcelSerializer).IsAssignableFrom(type)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());
            return services;
        }

        private static IServiceCollection AddObjectMapper(this IServiceCollection services, IEnumerable<Assembly> assembliesForSearch)
        {
            var _RoseConfigurations = services.BuildServiceProvider().GetService<RoseConfigurationOptions>();
            if (_RoseConfigurations.RegisterAutomapperProfiles)
            {
                services.AddAutoMapperProfiles(assembliesForSearch);
            }
            return services;
        }
        private static IServiceCollection AddUserInfoService(this IServiceCollection services,
            IEnumerable<Assembly> assembliesForSearch)
        {
            var _RoseConfigurations = services.BuildServiceProvider().GetService<RoseConfigurationOptions>();
            services.Scan(s => s.FromAssemblies(assembliesForSearch)
                .AddClasses(classes => classes.Where(type => type.Name == _RoseConfigurations.UserInfoServiceTypeName && typeof(IUserInfoService).IsAssignableFrom(type)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            return services;
        }
        private static IServiceCollection AddTranslator(this IServiceCollection services,
            IEnumerable<Assembly> assembliesForSearch)
        {
            var _RoseConfigurations = services.BuildServiceProvider().GetService<RoseConfigurationOptions>();
            services.Scan(s => s.FromAssemblies(assembliesForSearch)
                .AddClasses(classes => classes.Where(type => type.Name == _RoseConfigurations.Translator.TranslatorTypeName && typeof(ITranslator).IsAssignableFrom(type)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());
            return services;
        }


        private static IServiceCollection AddMessageBus(this IServiceCollection services,
            IEnumerable<Assembly> assembliesForSearch)
        {
            var _RoseConfigurations = services.BuildServiceProvider().GetService<RoseConfigurationOptions>();

            services.Scan(s => s.FromAssemblies(assembliesForSearch)
                .AddClasses(classes => classes.Where(type => type.Name == _RoseConfigurations.MessageBus.MessageConsumerTypeName && typeof(IMessageConsumer).IsAssignableFrom(type)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            services.Scan(s => s.FromAssemblies(assembliesForSearch)
             .AddClasses(classes => classes.Where(type => type.Name == _RoseConfigurations.Messageconsumer.MessageInboxStoreTypeName && typeof(IMessageInboxItemRepository).IsAssignableFrom(type)))
             .AsImplementedInterfaces()
             .WithSingletonLifetime());

            services.Scan(s => s.FromAssemblies(assembliesForSearch)
                .AddClasses(classes => classes.Where(type => type.Name.StartsWith(_RoseConfigurations.MessageBus.MessageBusTypeName) && typeof(ISendMessageBus).IsAssignableFrom(type)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            services.Scan(s => s.FromAssemblies(assembliesForSearch)
                .AddClasses(classes => classes.Where(type => type.Name.StartsWith(_RoseConfigurations.MessageBus.MessageBusTypeName) && typeof(IReceiveMessageBus).IsAssignableFrom(type)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());


            services.AddHostedService<IdempotentConsumerHostedService>();
            return services;
        }

        private static IServiceCollection AddPoolingPublisher(this IServiceCollection services,
            IEnumerable<Assembly> assembliesForSearch)
        {
            var _RoseConfigurations = services.BuildServiceProvider().GetService<RoseConfigurationOptions>();
            if (_RoseConfigurations.PoolingPublisher.Enabled)
            {
                services.Scan(s => s.FromAssemblies(assembliesForSearch)
                    .AddClasses(classes => classes.Where(type => type.Name == _RoseConfigurations.PoolingPublisher.OutBoxRepositoryTypeName && typeof(IOutBoxEventItemRepository).IsAssignableFrom(type)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime());
                services.AddHostedService<PoolingPublisherHostedService>();

            }
            return services;
        }

        private static IServiceCollection AddEntityChangeInterception(this IServiceCollection services,
            IEnumerable<Assembly> assembliesForSearch)
        {
            var _RoseConfigurations = services.BuildServiceProvider().GetService<RoseConfigurationOptions>();
            if (_RoseConfigurations.EntityChangeInterception.Enabled)
            {
                services.Scan(s => s.FromAssemblies(assembliesForSearch)
                    .AddClasses(classes => classes.Where(type => type.Name == _RoseConfigurations.EntityChangeInterception.
                        EntityChageInterceptorRepositoryTypeName && typeof(IEntityChageInterceptorItemRepository).IsAssignableFrom(type)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());
            }
            return services;
        }
    }
}
