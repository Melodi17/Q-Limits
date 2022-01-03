using q_limits.Modules;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace q_limits
{
    public class ModuleExceptAttribute : Attribute { }
    public static class ModuleService
    {
        public static List<IModule> KnownModules;
        public static List<Credential> KnownSuccessfulCredentials;

        static ModuleService()
        {
            KnownModules = new();
            KnownSuccessfulCredentials = new();

            var type = typeof(IModule);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p != type) /* Makes sure the type implements IModule and prevents original type (IModule) from being included */
                .Where(x => x.GetCustomAttribute<ModuleExceptAttribute>() == null) /* Used to prevent a module from being loaded if it has the ModuleExcept attribute */
                .Select(x =>
                {
                    return (IModule)Activator.CreateInstance(x);
                });

            KnownModules.AddRange(types);
        }

        public static void FindAssessLoadModule(ProgressContext progCtx, CommandLineOptions options)
        {
            // Extract and calculate possibilities
            var credGenTask = progCtx.AddTask("[gray][[Service]][/] Generating credentials", true, 10);

            CredentialContext credContext = new();
            List<string> usernames = new();
            List<string> passwords = new();

            if (options.MaxThreadCount > 1)
            {
                options.MaxThreadCount = 100;
            }

            if (options.Login != null)
            {
                usernames.Add(options.Login);
            }
            if (options.LoginFile != null)
            {
                if (File.Exists(options.LoginFile))
                {
                    var fileTask = progCtx.AddTask($"[gray][[Service]][/] Reading file {Path.GetFileName(options.LoginFile)}");
                    fileTask.MaxValue = 100;
                    fileTask.IsIndeterminate = true;
                    string line;
                    using (StreamReader file = new(options.LoginFile))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            usernames.Add(line);
                            if ((int)fileTask.MaxValue == (int)fileTask.Value) fileTask.MaxValue *= 2;
                            fileTask.Increment(1);
                        }
                    }

                    fileTask.Value = fileTask.MaxValue;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]File '{options.LoginFile}' was not found[/]");
                }
            }
            if (options.Password != null)
            {
                passwords.Add(options.Password);
            }
            if (options.PasswordFile != null)
            {
                if (File.Exists(options.PasswordFile))
                {
                    var fileTask = progCtx.AddTask($"[gray][[Service]][/] Reading file {Path.GetFileName(options.PasswordFile)}");
                    fileTask.MaxValue = 100;
                    fileTask.IsIndeterminate = true;
                    string line;
                    using (StreamReader file = new(options.PasswordFile))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            passwords.Add(line);
                            if ((int)fileTask.MaxValue == (int)fileTask.Value) fileTask.MaxValue *= 2;
                            fileTask.Increment(1);
                        }
                    }

                    fileTask.Value = fileTask.MaxValue;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]File '{options.PasswordFile}' was not found[/]");
                }
            }
            if (options.PasswordGeneration != null)
            {
                string[] splt = options.PasswordGeneration.Split(":");
                if (splt.Length == 3)
                {
                    //try
                    //{
                    int min = int.Parse(splt[0]);
                    int max = int.Parse(splt[1]);
                    bool lowercase = splt[2].Contains("a");
                    bool uppercase = splt[2].Contains("A");
                    bool numbers = splt[2].Contains("1");
                    bool symbols = splt[2].Contains("!");

                    string possiblechars = "";
                    if (lowercase)
                    {
                        possiblechars += "abcdefghijklmnopqrstuvwxyz";
                    }
                    if (uppercase)
                    {
                        possiblechars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    }
                    if (numbers)
                    {
                        possiblechars += "0123456789";
                    }
                    if (symbols)
                    {
                        possiblechars += "!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?~`";
                    }

                    if (options.PasswordGenerationXCharset != null)
                    {
                        foreach (char ch in options.PasswordGenerationXCharset)
                        {
                            if (!possiblechars.Contains(ch)) possiblechars += ch;
                        }
                    }

                    // TODO: Generate every possible combination with a progressbar and add it to 'passwords'
                    var genTask = progCtx.AddTask("Implementing charset", true, 100);
                    genTask.IsIndeterminate = true;
                    Dive(ref passwords, "", 0, possiblechars, min, max);
                    genTask.Value = genTask.MaxValue;
                    //}
                    //catch (Exception e)
                    //{
                    //    AnsiConsole.MarkupLine("[red]Generation (x) parameter failed[/]");
                    //    AnsiConsole.MarkupLine($"[gray]{e.Message}[/]");
                    //}
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Generation (x) parameter requires 3 chunks such as: '1:3:aA1!'[/]");
                }
            } // TODO: Finish dive algorithm

            credContext.Usernames = usernames.ToArray();
            credContext.Passwords = passwords.ToArray();

            credGenTask.Value = credGenTask.MaxValue;

            // Find correct module
            var findModTask = progCtx.AddTask("[gray][[Service]][/] Finding module", true, 10);

            bool func(IModule x) => x.ID.ToLower() == options.Module.ToLower();

            if (!KnownModules.Any(func)) // Ignore case
            {
                AnsiConsole.MarkupLine($"[red]Unknown module '{options.Module.ToLower()}'[/]");
                return;
            }

            IModule loadingModule = KnownModules.First(func);

            findModTask.Value = findModTask.MaxValue;
            loadingModule.Load(options, credContext, progCtx);
        }

        public static void ReportSuccess(string dest, Credential cred, string loginName = "login", string passName = "password")
        {
            KnownSuccessfulCredentials.Add(cred);
            AnsiConsole.MarkupLine($"[[[blue underline]{DateTime.Now}[/]]] Credentials retrieved for [blue]{dest}[/] > {loginName}: {(cred.Key != null ? $"[green]{cred.Key}[/]" : "[red]NULL[/]")}  {passName}: {(cred.Value != null ? $"[green]{cred.Value}[/]" : "[red]NULL[/]")}");
        }

        public static void Dive(ref List<string> arr, string prefix, int level, string validchars, int min, int max)
        {
            level += 1;
            foreach (char c in validchars)
            {
                string res = prefix + c;
                if (res.Length >= min && res.Length <= max)
                {
                    arr.Add(res);
                }
                if (level < max)
                {
                    Dive(ref arr, prefix + c, level, validchars, min, max);
                }
            }
        }
    }
}
