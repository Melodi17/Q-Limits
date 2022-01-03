using OpenPop.Pop3;
using Renci.SshNet;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace q_limits.Modules
{
    [ModuleExcept]
    public class POP3Module : IModule
    {
        public POP3Module()
        {
            Name = "POP3";
            ID = "pop3";
        }
        public override void Load(CommandLineOptions options, CredentialContext credContext, ProgressContext progCtx)
        {
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
                    Pop3Client client = new();
                    client.Connect(options.Destination, options.DestinationPort < 0 ? 995 : options.DestinationPort, true);
                    // TODO: Allow user to configure ssl
                    client.Authenticate(x.Key, x.Value);
                    client.Disconnect();
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
