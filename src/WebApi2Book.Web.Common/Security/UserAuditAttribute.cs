﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using WebApi2Book.Common.Logging;
using WebApi2Book.Common.Security;
using System.Threading;
using System.Web.Http.Controllers;

namespace WebApi2Book.Web.Common.Security
{
    public class UserAuditAttribute : ActionFilterAttribute
    {
        private readonly ILog _log;
        private readonly IUserSession _userSession;

        public UserAuditAttribute() : this(WebContainerManager.Get<ILogManager>(),
            WebContainerManager.Get<IUserSession>())
        {

        }
        public UserAuditAttribute(ILogManager logManager, IUserSession userSession)
        {
            _log = logManager.GetLog(typeof(UserAuditAttribute));
            _userSession = userSession;
        }

        public override bool AllowMultiple
        {
            get
            {
                return false;
            }
        }

        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            _log.Debug("Starting execution...");
            var userName = _userSession.Username;
            return Task.Run(() => AuditCurrentUser(userName), cancellationToken);
        }

        public void AuditCurrentUser(string username)
        {
            //Simulate long uadit process
            _log.InfoFormat("Action being executed by user={0}", username);
            Thread.Sleep(3000);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            _log.InfoFormat("Action executed by user={0}", _userSession.Username);
        }
    }
}
