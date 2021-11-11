using System;
using Spectre.Console;
using Melodi.IO;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Net;
using CommandLine;
using q_limits.Modules;

namespace q_limits
{
    class Program
    {
        static DateTime StartTime { get; }
        static Version Version { get; }

        static Program()
        {
            StartTime = DateTime.Now;
            Version = typeof(Program).Assembly.GetName().Version;
        }

        static void Main(string[] args)
        {
            _ = Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(ExecuteEngine)
                .WithNotParsed(errs =>
                {
                    if (!(errs.IsHelp() || errs.IsVersion()))
                    {
                        AnsiConsole.MarkupLine("[red]Insufficient/Invalid parameters are supplied, type 'q-limits --help' for help[/]");
                    }
                });
        }

        static void ExecuteEngine(CommandLineOptions options)
        {
            /*string[] lns = File.ReadAllLines(options.PasswordFile);
            File.WriteAllText(options.Destination, "");
            for (int i = 0; i < 100; i++)
            {
                File.AppendAllText(options.Destination, $"{Sha256HashModule.ComputeSha256Hash(lns[new Random().Next(lns.Length)])}\n");
            }
            return;*/
            AnsiConsole.MarkupLine($"Q-Limits [[Version [blue]{Version}[/]]]");
            AnsiConsole.MarkupLine("Made by [blue]Melodi[/] and [blue]Github[/]");
            AnsiConsole.MarkupLine($"Started at [blue]{StartTime}[/]");
            AnsiConsole.Write(new Rule().Centered());

            Thread.Sleep(250);

            AnsiConsole.Progress()
                .AutoRefresh(true) // Turn on auto refresh
                .AutoClear(false) // Do not remove the task list when done
                .HideCompleted(false) // Hide tasks as they are completed
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(), // Task description
                    new ProgressBarColumn {CompletedStyle = new(Color.Orange1)}, // Progress bar
                    new PercentageColumn(), // Percentage
                    new RemainingTimeColumn(), // Remaining time
                    new SpinnerColumn {Style = new(Color.Blue)}, // Spinner
                }).Start(ctx => ModuleService.FindAssessLoadModule(ctx, options));

            ShowStatistics();
        }

        static void ShowStatistics()
        {
            DateTime finishTime = DateTime.Now;

            AnsiConsole.Write(new Rule().Centered());
            AnsiConsole.MarkupLine($"Finished at [blue]{finishTime}[/], took [blue]{finishTime - StartTime}[/], found [blue]{ModuleService.KnownSuccessfulCredentials.Count}[/]");
        }
    }
}
