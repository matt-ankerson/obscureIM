using Microsoft.Owin;
using Owin;
[assembly: OwinStartup(typeof(obscureIM.Web.Startup))]

namespace obscureIM.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}