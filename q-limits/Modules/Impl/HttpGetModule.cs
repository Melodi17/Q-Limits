using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Melodi.Networking;
using q_limits.CombinationProviders;
using q_limits.CombinationProviders.Impl;
using Spectre.Console;

namespace q_limits.Modules.Impl
{
    public class HttpGetModule : Module
    {
        private static readonly int[] blackList = { 401, 403, 407, 511 };
        private static readonly UsernamePasswordCombinationProvider combinationProvider = new();
        public HttpGetModule(): base("Http Get", "http-get") { }
        public override ICombinationProvider GetCombinationProvider() => combinationProvider;

        public override void Load(CommandLineOptions options, ProgressContext progCtx)
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

            int combs = combinationProvider.GetCombinationCount();

            ProgressTask mainTask = progCtx.AddTask("[gray][[Module]][/] Breaking limits", true, combs);
            ParallelExecutor.ForEachAsync(options.MaxThreadCount, combinationProvider.EnumerateCombinations(), x => 
            {
                try
                {
                    WebRequest web = WebRequest.Create(options.Destination);
                    web.Proxy = proxy;

                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(x.Username + ":" + x.Password));

                    web.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                    web.Credentials = new NetworkCredential(x.Username, x.Password);

                    web.GetResponse();

                    ModuleService.ReportSuccess(options.Destination, x);
                }
                catch (Exception e) 
                {
                    if (blackList.All(y => !e.Message.Contains("(" + y + ")")))
                        ModuleService.ReportSuccess(options.Destination, x);
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
