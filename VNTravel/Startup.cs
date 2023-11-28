using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VNTravel.Startup))]
namespace VNTravel
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
