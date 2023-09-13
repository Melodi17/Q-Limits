using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Renci.SshNet;
using Spectre.Console;

namespace q_limits.Modules.Impl
{
    public class SSHModule : Module
    {
        public SSHModule()
        {
            this.Name = "SSH";
            this.ID = "ssh";
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
                    SshClient cSSH = new(options.Destination, options.DestinationPort < 0 ? 22 : options.DestinationPort, x.Key, x.Value);
                    cSSH.Connect();
                    cSSH.Disconnect();
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
