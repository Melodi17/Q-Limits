using System;
using Spectre.Console;
using Melodi.IO;
using System.Threading;
using System.Collections.Generic;
using System.Net;

namespace q_limits
{
    class Program
    {
        static DateTime startTime;
        static string Version = "2.0.0";
        static void Main(string[] args)
        {
            Console.CancelKeyPress += (_, _) =>
            {
                End();
            };

                startTime = DateTime.Now;
            AnsiConsole.MarkupLine($"Q-Limits [[Version [blue]{Version}[/]]]");
            AnsiConsole.MarkupLine("Made by [blue]Melodi[/] and [blue]Github[/]");
            AnsiConsole.MarkupLine($"Started at [blue]{startTime}[/]");
            AnsiConsole.Write(new Rule().Centered());

            Thread.Sleep(500);

            Dictionary<string, string> argD = ArgumentParser.ParseString(string.Join(" ", args));
            if (argD.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Parameters are required, type 'q-limits -h' or help[/]");
            }
            else if (argD.ContainsKey("h"))
            {
                AnsiConsole.MarkupLine("[blue underline]Help Menu[/]");
                AnsiConsole.MarkupLine("[blue]Examples[/]");
                Console.WriteLine("q-limits -m ssh -d 127.0.0.1 -l admin -P wordlist.txt");
                Console.WriteLine("q-limits -m http-get-form -d 192.168.1.45:7312 -L usernames.txt -x 4:4:1 -s Welcome -f Denied");
                Console.WriteLine("q-limits -m proxy -d https://www.google.com -l user -p password");
                AnsiConsole.MarkupLine("[blue]Parameters[/]");
                Console.WriteLine("-h                           Shows help");
                Console.WriteLine("-H                           Shows full help");
                Console.WriteLine("-m <mode>                    Sets attack mode");
                Console.WriteLine("-d <destination>             Sets destination");
                Console.WriteLine("-l <login>                   Login (username) to try");
                Console.WriteLine("-L <file>                    File of logins to try");
                Console.WriteLine("-p <password>                Password to try");
                Console.WriteLine("-P <file>                    File of password to try");
                Console.WriteLine("-x <min>:<max>:<1|a|A|!>     Password generation from execute");
                Console.WriteLine("-n <login>:<password>        Proxy credentials");
                Console.WriteLine("-t <threads>                 Amount of threads");
            }
            else if (argD.ContainsKey("m") && argD.ContainsKey("d"))
            {
                AnsiConsole.Progress()
                    .AutoRefresh(true) // Turn on auto refresh
                    .AutoClear(false)   // Do not remove the task list when done
                    .HideCompleted(false)   // Hide tasks as they are completed
                    .Columns(new ProgressColumn[]
                    {
                        new TaskDescriptionColumn(),    // Task description
                        new ProgressBarColumn() {CompletedStyle = new(Color.Orange1)},        // Progress bar
                        new PercentageColumn(),         // Percentage
                        new RemainingTimeColumn(),      // Remaining time
                        new SpinnerColumn(),            // Spinner
                    }).Start(ctx => ModuleService.FindAssessLoadModule(ctx, argD));

                End();
            }
        }

        static void End()
        {
            DateTime finishTime = DateTime.Now;

            AnsiConsole.Write(new Rule().Centered());
            AnsiConsole.MarkupLine($"Finished at [blue]{finishTime}[/], took [blue]{finishTime - startTime}[/], found [blue]{ModuleService.KnownSuccessfulCredentials.Count}[/]");
        }
    }
}
