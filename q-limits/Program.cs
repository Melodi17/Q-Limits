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
        static string Version = "2.0.3"; // TODO: Remember to update the version all the time
        static void Main(string[] args)
        {
            Console.CancelKeyPress += (_, _) =>
            {
                End();
                // TODO: Make the time bar appear after progressbar somehow
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
                // TODO: Write help menu (base of README.md)
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
