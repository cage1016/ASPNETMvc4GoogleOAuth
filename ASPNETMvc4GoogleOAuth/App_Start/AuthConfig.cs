using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using ASPNETMvc4GoogleOAuth.Models;
using System.Configuration;

namespace ASPNETMvc4GoogleOAuth
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            //OAuthWebSecurity.RegisterFacebookClient(
            //    appId: "",
            //    appSecret: "");

            //OAuthWebSecurity.RegisterGoogleClient();
            string clientId = ConfigurationManager.AppSettings["clientId"] as string;
            string clientSecret = ConfigurationManager.AppSettings["clientSecret"] as string;

            OAuthWebSecurity.RegisterClient(new GoogleClient(clientId, clientSecret), "Google", null);
        }
    }
}
