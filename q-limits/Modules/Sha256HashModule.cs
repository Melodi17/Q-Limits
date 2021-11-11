using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace q_limits.Modules
{
    public class Sha256HashModule : IModule
    {
        public Sha256HashModule()
        {
            Name = "SHA256 Hash";
            ID = "sha256-hash";
        }
        public override void Load(CommandLineOptions options, CredentialContext credContext, ProgressContext progCtx)
        {
            if (!File.Exists(options.Destination))
            {
                AnsiConsole.MarkupLine("[red]options.Destinationination file does not exist[/]");
                return;
            }

            var buildTask = progCtx.AddTask("[gray][[Module]][/] Building list into mem", true, credContext.Combinations);
            List<Credential> possibilities = new();

            foreach (var hash in File.ReadAllLines(options.Destination))
            {
                foreach (var password in credContext.Passwords)
                {
                    possibilities.Add(new(hash, password));
                    buildTask.Increment(1);
                }
            }

            buildTask.Value = buildTask.MaxValue;
            var mainTask = progCtx.AddTask("[gray][[Module]][/] Breaking limits", true, possibilities.Count);
            ParallelExecutor.ForEachAsync(options.MaxThreadCount, possibilities, x => 
            {
                try
                {
                    if (x.Key.ToLower() == ComputeSha256Hash(x.Value).ToLower())
                    {
                        ModuleService.ReportSuccess(Path.GetFileName(options.Destination), x, "hash", "value");
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

        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
