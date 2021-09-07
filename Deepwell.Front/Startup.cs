using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Deepwell.Front.Startup))]
namespace Deepwell.Front
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
