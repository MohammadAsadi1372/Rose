using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Data.SqlClient;
using Rose.EndPoints.Web.Filters;
using Rose.EndPoints.Web.Middlewares.ApiExceptionHandler;
using Microsoft.Extensions.DependencyInjection;
using Rose.Utilities.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Rose.EndPoints.Web.StartupExtentions
{
    public static class AddMvcConfigurationExtentions
    {
        public static IServiceCollection AddRoseMvcServices(this IServiceCollection services,
            IConfiguration configuration, Action<MvcOptions> mvcOptions = null)
        {
            var RoseConfigurations = new RoseConfigurationOptions();
            configuration.GetSection(RoseConfigurations.SectionName).Bind(RoseConfigurations);
            services.AddSingleton(RoseConfigurations);
            services.AddControllersWithViews(mvcOptions == null ? (options =>
            {
                options.Filters.Add(typeof(TrackActionPerformanceFilter));
            }) : mvcOptions).AddRazorRuntimeCompilation()
            .AddFluentValidation();

            if (RoseConfigurations?.Session?.Enable == true)
                services.AddSession();

            services.AddRoseDependencies(RoseConfigurations.AssmblyNameForLoad.Split(','));

            return services;
        }

        public static void UseRoseMvcConfigure(this IApplicationBuilder app, Action<IEndpointRouteBuilder> configur, RoseConfigurationOptions configuration, IWebHostEnvironment env)
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
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            if (configuration?.Session?.Enable == true)
                app.UseSession();

            app.UseEndpoints(configur);
        }
    }
}
