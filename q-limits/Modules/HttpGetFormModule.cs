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
    public class HttpGetFormModule : IModule
    {
        public HttpGetFormModule()
        {
            Name = "Http Get Form";
            ID = "http-get-form";
            Help = "NONONO";
        }
        public override void Load(string dest, int thCount, CredentialContext credContext, Dictionary<string, string> argD, ProgressContext progCtx)
        {
            WebProxy proxy = null;
            if (ProxyManager.ProxyRequired)
            {
                AnsiConsole.MarkupLine("[gray]Proxy detected[/]");
                if (argD.ContainsKey("n"))
                {
                    string[] splt = argD["n"].Split(":", 2);
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
            ParallelExecutor.ForEachAsync(thCount, possibilities, x => 
            {
                try
                {
                    WebClient web = new();
                    web.Proxy = proxy;

                    string cont = web.DownloadString(argD["d"].Replace("{LOGIN}", x.Key).Replace("{PASSWORD}", x.Value));
                    bool contPassed = true;

                    if (argD.ContainsKey("s"))
                    {
                        if (cont.Contains(argD["s"]))
                        {
                            contPassed = true;
                        }
                    }
                    if (argD.ContainsKey("f"))
                    {
                        if (cont.Contains(argD["f"]))
                        {
                            contPassed = false;
                        }
                    }

                    if (contPassed)
                    {
                        ModuleService.ReportSuccess(dest, x);
                    }
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
