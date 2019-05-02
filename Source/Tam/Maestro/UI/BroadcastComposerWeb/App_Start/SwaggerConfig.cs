using System.Web.Http;
using WebActivatorEx;
using BroadcastComposerWeb;
using Swashbuckle.Application;
using System;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace BroadcastComposerWeb
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                    {
                        c.SingleApiVersion("v1", "BroadcastComposerWeb");
                        c.UseFullTypeNameInSchemaIds();
                        c.IncludeXmlComments($@"{AppDomain.CurrentDomain.BaseDirectory}\bin\BroadcastComposerWeb.xml");
                    })
                .EnableSwaggerUi(c => {});
        }
    }
}
