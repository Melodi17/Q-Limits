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
    public class TelnetModule : IModule
    {
        public TelnetModule()
        {
            Name = "Telnet";
            ID = "telnet";
            Help = "NONONO";
        }
        public override void Load(string dest, int thCount, CredentialContext credContext, Dictionary<string, string> argD, ProgressContext progCtx)
        {
            string[] splt = dest.Split(":", 2);
            string servAdd = splt[0];
            int port = 23;
            if (splt.Length > 1)
            {
                if (!int.TryParse(splt[1], out port))
                {
                    AnsiConsole.MarkupLine("[red]Specified port was not a number[/]");
                    return;
                }
            }
            TCPConnectionClient client = new(servAdd, port);
            client.onConnect = () =>
            {
                Console.WriteLine("Connected");
            };
            client.onDisconnect = () =>
            {
                Console.WriteLine("Connected");
            };
            client.onConnectFailed = () =>
            {
                Console.WriteLine("Connected");
            };
            client.onMessage = buffer =>
            {
                Console.WriteLine(buffer);
            };
            client.Start();
            while (true)
            {
                client.Send(Console.ReadLine());
            }
            
            
            return;
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
