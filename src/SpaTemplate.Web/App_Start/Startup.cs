using Microsoft.Owin;
using Owin;
using SpaTemplate;

[assembly: OwinStartup(typeof(Startup))]

namespace SpaTemplate
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}