using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using System.Data.SqlClient;
using Rose.EndPoints.Web.Filters;
using Rose.EndPoints.Web.Middlewares.ApiExceptionHandler;
using Microsoft.Extensions.DependencyInjection;
using Rose.Utilities.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Rose.EndPoints.Web.StartupExtentions
{
    public static class AddApiConfigurationExtentions
    {
        public static IServiceCollection AddRoseApiServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            var _RoseConfigurations = new RoseConfigurationOptions();
            configuration.GetSection(_RoseConfigurations.SectionName).Bind(_RoseConfigurations);
            services.AddSingleton(_RoseConfigurations);
            services.AddScoped<ValidateModelStateAttribute>();
            services.AddControllers(options =>
            {
                options.Filters.AddService<ValidateModelStateAttribute>();
                options.Filters.Add(typeof(TrackActionPerformanceFilter));
            }).AddFluentValidation();

            services.AddEndpointsApiExplorer();

            services.AddRoseDependencies(_RoseConfigurations.AssmblyNameForLoad.Split(','));

            AddSwagger(services);
            return services;
        }

        private static void AddSwagger(IServiceCollection services)
        {
            var _RoseConfigurations = services.BuildServiceProvider().GetService<RoseConfigurationOptions>();
            if (_RoseConfigurations?.Swagger?.Enabled == true && _RoseConfigurations.Swagger.SwaggerDoc != null)
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc(_RoseConfigurations.Swagger.SwaggerDoc.Name, new OpenApiInfo { Title = _RoseConfigurations.Swagger.SwaggerDoc.Title, Version = _RoseConfigurations.Swagger.SwaggerDoc.Version });
                });
            }
        }
        public static void UseRoseApiConfigure(this IApplicationBuilder app, RoseConfigurationOptions configuration, IWebHostEnvironment env)
        {
            app.UseApiExceptionHandler(options =>
            {
                options.AddResponseDetails = (context, ex, error) =>
                {
                    if (ex.GetType().Name == typeof(SqlException).Name)
                    {
                        error.Detail = "Exception was a database exception!";
                    }
                };
                options.DetermineLogLevel = ex =>
                {
                    if (ex.Message.StartsWith("cannot open database", StringComparison.InvariantCultureIgnoreCase) ||
                        ex.Message.StartsWith("a network-related", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return LogLevel.Critical;
                    }
                    return LogLevel.Error;
                };
            });

            app.UseStatusCodePages();
            if (configuration.Swagger != null && configuration.Swagger.SwaggerDoc != null)
            {

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(configuration.Swagger.SwaggerDoc.URL, configuration.Swagger.SwaggerDoc.Title);
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
            });
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }




    }
}
