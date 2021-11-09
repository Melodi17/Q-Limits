using q_limits.Modules;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace q_limits
{
    public static class ModuleService
    {
        public static List<IModule> KnownModules;
        public static List<Credential> KnownSuccessfulCredentials;
        
        static ModuleService()
        {
            KnownModules = new();
            KnownSuccessfulCredentials = new();

            KnownModules.Add(new HttpProxyModule());
            KnownModules.Add(new Sha256HashModule());
            KnownModules.Add(new MD5HashModule());
            KnownModules.Add(new SSHModule());
            KnownModules.Add(new HttpGetModule());
            KnownModules.Add(new HttpGetFormModule());
            KnownModules.Add(new FTPModule());
        }

        public static void FindAssessLoadModule(ProgressContext progCtx, Dictionary<string, string> argD)
        {
            // Find destination and mode
            var argAssessTask = progCtx.AddTask("[gray][[Service]][/] Assessing arguments", true, 10);

            if (!argD.ContainsKey("d"))
                throw new Exception("Destination (d) parameter was missing");

            string destination = argD["d"];

            if (!argD.ContainsKey("m"))
                throw new Exception("Mode (m) parameter was missing");

            string mode = argD["m"];

            int thCount = 100;
            if (argD.ContainsKey("t"))
            {
                if (!int.TryParse(argD["t"], out thCount))
                {
                    AnsiConsole.MarkupLine("[red]Thread max count (t) parameter was not a number[/]");
                }
            }

            argAssessTask.Value = argAssessTask.MaxValue;

            // Extract and calculate possibilities
            var credGenTask = progCtx.AddTask("[gray][[Service]][/] Generating credentials", true, 10);

            CredentialContext credContext = new();
            List<string> usernames = new();
            List<string> passwords = new();

            if (argD.ContainsKey("l"))
            {
                usernames.Add(argD["l"]);
            }
            if (argD.ContainsKey("L"))
            {
                if (File.Exists(argD["L"]))
                {
                    usernames.AddRange(File.ReadAllLines(argD["L"]));
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]File '{argD["L"]}' was not found[/]");
                }
            }
            if (argD.ContainsKey("p"))
            {
                passwords.Add(argD["p"]);
            }
            if (argD.ContainsKey("P"))
            {
                if (File.Exists(argD["P"]))
                {
                    string[] flCont = File.ReadAllLines(argD["P"]);
                    passwords.AddRange(flCont);
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]File '{argD["P"]}' was not found[/]");
                }
            }
            if (argD.ContainsKey("x"))
            {
                string[] splt = argD["x"].Split(":");
                if (splt.Length == 3)
                {
                    try
                    {
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
                        
                        // TODO: Generate every possible combination with a progressbar and add it to 'passwords'
                    }
                    catch (Exception)
                    {
                        AnsiConsole.MarkupLine("[red]Generation (x) parameter failed[/]");
                    }
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

            bool func(IModule x) => x.ID.ToLower() == mode.ToLower();

            if (!KnownModules.Any(func)) // Ignore case
                throw new Exception($"Unknown module '{mode.ToLower()}'"); // TODO: Write output instead

            IModule loadingModule = KnownModules.First(func);

            findModTask.Value = findModTask.MaxValue;
            loadingModule.Load(destination, thCount, credContext, argD, progCtx);
        }

        public static void ReportSuccess(string dest, Credential cred, string loginName = "login", string passName = "password")
        {
            KnownSuccessfulCredentials.Add(cred);
            AnsiConsole.MarkupLine($"[[[blue underline]{DateTime.Now}[/]]] Credentials retrieved for [blue]{dest}[/] > {loginName}: {(cred.Key != null ? $"[green]{cred.Key}[/]" : "[red]NULL[/]")}  {passName}: {(cred.Value != null ? $"[green]{cred.Value}[/]" : "[red]NULL[/]")}");
        }
    }
}
