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
    public class MD5HashModule : IModule
    {
        public MD5HashModule()
        {
            Name = "MD5 Hash";
            ID = "md5-hash";
        }
        public override void Load(CommandLineOptions options, CredentialContext credContext, ProgressContext progCtx)
        {
            if (!File.Exists(options.Destination))
            {
                AnsiConsole.MarkupLine("[red]Destination file does not exist[/]");
                return;
            }

            var buildTask = progCtx.AddTask("[gray][[Module]][/] Building list into mem", true, credContext.Combinations);
            List<string> hashes = File.ReadAllLines(options.Destination).ToList();
            int possibilities = credContext.Passwords.Length;

            buildTask.Value = buildTask.MaxValue;
            var mainTask = progCtx.AddTask("[gray][[Module]][/] Breaking limits", true, possibilities);
            ParallelExecutor.ForEachAsync(options.MaxThreadCount, credContext.Passwords, x =>
            {
                try
                {
                    foreach (string hash in hashes.ToArray())
                    {
                        if (hash.ToLower() == ComputeMD5Hash(x).ToLower())
                        {
                            ModuleService.ReportSuccess(Path.GetFileName(options.Destination), new(hash, x), "hash", "value");
                            lock (hashes)
                            {
                                hashes.Remove(hash);
                            }
                        }
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

        public static string ComputeMD5Hash(string input)
        {
            // Use input string to calculate MD5 hash
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
