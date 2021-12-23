using Rose.Core.ApplicationServices.Events;
using Rose.Utilities;
using Microsoft.AspNetCore.Http;
using Rose.Core.Contracts.ApplicationServices.Commands;
using Rose.Core.Contracts.ApplicationServices.Queries;

namespace Rose.EndPoints.Web.Extentions
{
    public static class HttpContextExtentions
    {
        public static ICommandDispatcher CommandDispatcher(this HttpContext httpContext) =>
            (ICommandDispatcher)httpContext.RequestServices.GetService(typeof(ICommandDispatcher));

        public static IQueryDispatcher QueryDispatcher(this HttpContext httpContext) =>
            (IQueryDispatcher)httpContext.RequestServices.GetService(typeof(IQueryDispatcher));
        public static IEventDispatcher EventDispatcher(this HttpContext httpContext) =>
            (IEventDispatcher)httpContext.RequestServices.GetService(typeof(IEventDispatcher));
        public static RoseServices RoseApplicationContext(this HttpContext httpContext) =>
            (RoseServices)httpContext.RequestServices.GetService(typeof(RoseServices));
    }
}
