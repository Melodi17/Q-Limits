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
    public class MD5HashModule : Module
    {
        private static readonly HashedCombinationProvider provider = new(ComputeMD5Hash);
        public MD5HashModule() : base("MD5 Hash", "md5-hash") { }

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
            return sb.ToString().ToLower();
        }
    }
}
