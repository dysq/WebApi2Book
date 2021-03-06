﻿using JwtAuthForWebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using WebApi2Book.Common.Logging;
using WebApi2Book.Common.Security;
using WebApi2Book.Web.Api.LegacyProcessing;
using WebApi2Book.Web.Api.Security;
using WebApi2Book.Web.Common;

namespace WebApi2Book.Web.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            ConfigureFormatters();
            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            var logManager = WebContainerManager.Get<ILogManager>();
            var userSession = WebContainerManager.Get<IUserSession>();
            GlobalConfiguration.Configuration.MessageHandlers.Add(new BasicAuthenticationMessageHandler(logManager,
                WebContainerManager.Get<IBasicSecurityService>()));

            GlobalConfiguration.Configuration.MessageHandlers
                .Add(new TaskDataSecurityMessageHandler(logManager, userSession));

            var builder = new SecurityTokenBuilder();
            var reader = new ConfigurationReader();
            GlobalConfiguration.Configuration.MessageHandlers.Add(
                new JwtAuthenticationMessageHandler
                {
                    AllowedAudience = reader.AllowedAudience,
                    Issuer = reader.Issuer,
                    SigningToken = builder.CreateFromKey(reader.SymmetricKey)
                });

            GlobalConfiguration.Configuration.MessageHandlers.Add(
                new PagedTaskDataSecurityMessageHandler(logManager, userSession));
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            if (exception != null)
            {
                var log = WebContainerManager.Get<ILogManager>().GetLog(typeof(WebApiApplication));
                log.Error("Unhandled exception", exception);
            }
        }

        private void ConfigureFormatters()
        {
            var legacyFormatter = (MediaTypeFormatter)WebContainerManager.Get<ILegacyMessageTypeFormatter>();
            GlobalConfiguration.Configuration.Formatters.Insert(0, legacyFormatter);
        }
    }
}
