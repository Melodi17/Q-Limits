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
    public class FTPModule : IModule
    {
        public FTPModule()
        {
            Name = "FTP";
            ID = "ftp";
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
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(options.Destination);
                    request.Proxy = proxy;
                    request.Credentials = new NetworkCredential(x.Key, x.Value);
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
