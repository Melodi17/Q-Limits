using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using q_limits.CombinationProviders;
using q_limits.CombinationProviders.Impl;
using Spectre.Console;

namespace q_limits.Modules.Impl
{
    public class Sha256HashModule : Module
    {
        private static readonly HashedCombinationProvider provider = new(ComputeSha256Hash);
        public Sha256HashModule() : base("SHA256 Hash", "sha256-hash") { }
        
        public override ICombinationProvider GetCombinationProvider() => provider;
        
        public override void Load(CommandLineOptions options, ProgressContext progCtx)
        {
            if (!File.Exists(options.Destination))
            {
                AnsiConsole.MarkupLine("[red]Destination file does not exist[/]");
                return;
            }
            
            provider.SetHashCollection(File.ReadLines(options.Destination));

            int combs = provider.GetCombinationCount();
            
            string filename = Path.GetFileName(options.Destination);

            ProgressTask mainTask = progCtx.AddTask("[gray][[Module]][/] Breaking limits", true, combs);
            ParallelExecutor.ForEachAsync(options.MaxThreadCount, provider.EnumerateCombinations(), x => 
            {
                try
                {
                    if (x.Password.Equals(x.Hash, StringComparison.OrdinalIgnoreCase))
                    {
                        ModuleService.ReportSuccess(filename, x);
                        provider.RemoveHash(x.Hash);
                    }
                }
                catch (Exception)
                {
                    /* Don't Care */
                }

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
                StringBuilder builder = new();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
