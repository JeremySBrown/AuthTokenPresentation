using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ApiFinal.Datasource;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace ApiFinal.Providers
{
    public class CustomAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public async override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId;
            string clientSecret;

            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }

            if (context.ClientId == null)
            {
                context.SetError("invalid_clientId", "Client ID is required.");
                return;
            }
            var client = await SimpleIdentityManager.FindClientAsync(clientId);
            if (client == null)
            {
                context.SetError("invalid_clientId", "Client is not registered.");
                return;
            }

            if (client.ApplicationType == ApplicationTypes.NativeClient)
            {
                if (string.IsNullOrWhiteSpace(clientSecret))
                {
                    context.SetError("invalid_clientId", "Client Secret was not provided.");
                    return;
                }

                if (client.Secret != PasswordHelper.HashString(clientSecret))
                {
                    context.SetError("invalid_clientId", "Client Secret is invalid.");
                    return;
                }
            }

            if (!client.Active)
            {
                context.SetError("invalid_clientId", "Client is inactive.");
                return;
            }

            context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin);
            context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString());

            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var allowedOrigin = (context.OwinContext.Get<string>("as:clientAllowedOrigin")) ?? "*";
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            // validate user here
            var user = await SimpleIdentityManager.ValidateUserAsync(context.UserName, context.Password);
            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password are incorrect.");
                return;
            }

            // if successful add all claims for user
            var claims = new ClaimsIdentity(context.Options.AuthenticationType);
            claims.AddClaim(new Claim("sub", context.UserName));
            foreach (var role in user.Roles)
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var props = new AuthenticationProperties(new Dictionary<string, string>
                                                     {
                                                         {
                                                             "as:client_id", context.ClientId ?? string.Empty
                                                         },
                                                         {
                                                             "userName", context.UserName
                                                         }
                                                     });
            var ticket = new AuthenticationTicket(claims, props);

            context.Validated(ticket);

        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public override async Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
            var currentClient = context.ClientId;

            if (originalClient != currentClient)
            {
                context.SetError("invalid_clientId", "Refresh token not valid for the current client");
                return;
            }

            // Refresh Identity
            var userName = context.Ticket.Properties.Dictionary["userName"];
            var user = await SimpleIdentityManager.FindUserAsync(userName);
            if (user == null)
            {
                context.SetError("invalid_grant", "User does not exist");
                return;
            }

            var claimsIdentity = new ClaimsIdentity(context.Options.AuthenticationType);
            claimsIdentity.AddClaim(new Claim("sub", userName));
            foreach (var role in user.Roles)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var roles = string.Join(",", user.Roles.Select(r => r).ToArray());



            var props = new AuthenticationProperties(new Dictionary<string, string>
                                                     {
                                                         {
                                                             "as:client_id", context.ClientId ?? string.Empty
                                                         },
                                                         {
                                                             "userName", userName
                                                         },
                                                         {
                                                             "role", roles
                                                         }
                                                     });
            var ticket = new AuthenticationTicket(claimsIdentity, props);

            context.Validated(ticket);

        }
    }
}
