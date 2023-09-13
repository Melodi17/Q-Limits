using q_limits.Modules;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using q_limits.CombinationProviders;
using Module = q_limits.Modules.Module;

namespace q_limits
{
    public class ModuleExceptAttribute : Attribute { }
    public static class ModuleService
    {
        public static readonly List<Module> KnownModules;
        public static readonly List<ICombination> KnownSuccessfulCredentials;

        static ModuleService()
        {
            KnownModules = new();
            KnownSuccessfulCredentials = new();

            Type type = typeof(Module);
            IEnumerable<Module> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p != type) /* Makes sure the type implements IModule and prevents original type (IModule) from being included */
                .Where(x => x.GetCustomAttribute<ModuleExceptAttribute>() == null) /* Used to prevent a module from being loaded if it has the ModuleExcept attribute */
                .Select(x =>
                {
                    return (Module)Activator.CreateInstance(x);
                });

            KnownModules.AddRange(types);
        }

        public static void FindAssessLoadModule(ProgressContext progCtx, CommandLineOptions options)
        {
            ProgressTask findModTask = progCtx.AddTask("[gray][[Service]][/] Finding module", true, 10);

            bool Func(Module x) => x.ID.ToLower() == options.Module.ToLower();
            
            if (!KnownModules.Exists(Func))
            {
                AnsiConsole.MarkupLine($"[red]Module '{options.Module}' was not found[/]");
                return;
            }
            
            Module module = KnownModules.Find(Func);
            
            ICombinationProvider provider = module.GetCombinationProvider();
            
            findModTask.Value = findModTask.MaxValue;

            // Extract and calculate possibilities
            ProgressTask credGenTask = progCtx.AddTask("[gray][[Service]][/] Generating credentials", true, 10);

            if (options.MaxThreadCount < 1)
                options.MaxThreadCount = 100;

            if (options.Login != null)
                provider.SetLogin(options.Login);
            
            if (options.LoginFile != null)
            {
                if (File.Exists(options.LoginFile))
                {
                    ProgressTask fileTask = progCtx.AddTask($"[gray][[Service]][/] Reading file {Path.GetFileName(options.LoginFile)}");
                    fileTask.MaxValue = 1;
                    fileTask.IsIndeterminate = true;

                    provider.SetLoginCollection(File.ReadLines(options.LoginFile));

                    fileTask.Value = fileTask.MaxValue;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]File '{options.LoginFile}' was not found[/]");
                    return;
                }
            }
            
            if (options.Password != null)
                provider.SetPassword(options.Password);
            
            if (options.PasswordFile != null)
            {
                if (File.Exists(options.PasswordFile))
                {
                    ProgressTask fileTask = progCtx.AddTask($"[gray][[Service]][/] Reading file {Path.GetFileName(options.PasswordFile)}");
                    fileTask.MaxValue = 100;
                    fileTask.IsIndeterminate = true;

                    provider.SetPasswordCollection(File.ReadLines(options.PasswordFile));

                    fileTask.Value = fileTask.MaxValue;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]File '{options.PasswordFile}' was not found[/]");
                    return;
                }
            }
            
            credGenTask.Value = credGenTask.MaxValue;
            
            
            // Start cracking
            module.Load(options, progCtx);
        }

        public static void ReportSuccess(string dest, ICombination comb)
        {
            KnownSuccessfulCredentials.Add(comb);

            string dict = comb
                .GetFields()
                .Aggregate("", (current, pair) => current + $"{pair.Key}: [green]{pair.Value}[/]  ");

            AnsiConsole.MarkupLine($"[[[blue underline]{DateTime.Now}[/]]] Credentials retrieved for [blue]{dest}[/] > {dict}");
        }
    }
}
