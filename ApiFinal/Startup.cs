using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using ApiFinal.Providers;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Owin;

namespace ApiFinal
{
    [assembly: OwinStartup(typeof(ApiFinal.Startup))]
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureOAuth(app);

            HttpConfiguration configuration = new HttpConfiguration();

            WebApiConfiguration.Register(configuration);

            app.UseCors(CorsOptions.AllowAll);
            app.UseWebApi(configuration);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions oAuthOptions = new OAuthAuthorizationServerOptions
                                                           {
                                                               AllowInsecureHttp = true,
                                                               TokenEndpointPath = new PathString("/token"),
                                                               AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(10),
                                                               Provider = new CustomAuthorizationServerProvider(),
                                                               RefreshTokenProvider = new CustomRefreshTokenProvider()
                                                           };

            app.UseOAuthAuthorizationServer(oAuthOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
    }
}
