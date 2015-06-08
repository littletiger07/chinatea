using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ChinaTea.Startup))]
namespace ChinaTea
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
