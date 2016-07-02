using Esports.space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Esports
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            SportMatchManager.instance.Start();

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string path = Request.Url.LocalPath;
            if (!path.Contains(".asmx"))
            {
                if (path.Contains("/api"))
                {
                    int index = path.IndexOf("/api") + 4;
                    if (index == path.Length)
                        path += ".asmx";
                    else
                        path = path.Substring(0, index) + ".asmx?op=" + path.Substring(index + 1);
                    Context.RewritePath(path);
                }
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}