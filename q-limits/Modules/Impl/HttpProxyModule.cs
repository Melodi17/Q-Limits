using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using q_limits.CombinationProviders;
using q_limits.CombinationProviders.Impl;
using Spectre.Console;

namespace q_limits.Modules.Impl
{
    public class HttpProxyModule : Module
    {
        private static readonly UsernamePasswordCombinationProvider combinationProvider = new();
        public HttpProxyModule() : base("Http Proxy", "http-proxy") { }

        public override ICombinationProvider GetCombinationProvider() => combinationProvider;

        public override void Load(CommandLineOptions options, ProgressContext progCtx)
        {
            var det = WebRequest.GetSystemWebProxy();
            Uri proxyurl = det.GetProxy(new Uri(options.Destination));

            if (proxyurl == null)
            {
                AnsiConsole.MarkupLine("[red]No proxy detected[/]");
                return;
            }

            int combs = combinationProvider.GetCombinationCount();
            
            var mainTask = progCtx.AddTask("[gray][[Module]][/] Breaking limits", true, combs);
            ParallelExecutor.ForEachAsync(options.MaxThreadCount, combinationProvider.EnumerateCombinations(), x => 
            {
                try
                {
                    WebProxy webProxy = new(proxyurl, true);
                    webProxy.Credentials = new NetworkCredential(x.Username, x.Password);

                    WebRequest web = WebRequest.Create(options.Destination);
                    web.Proxy = webProxy;

                    web.GetResponse();

                    ModuleService.ReportSuccess(options.Destination, x);
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
