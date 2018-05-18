using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LibConsole;
using static LibConsole.PrettyPrint;
using static LibConsole.PrettyRead;

namespace LibExerciseRunner
{
    using Serilog;
    using Serilog.Core;

    public partial class Runner
    {
        private static readonly Finder Finder = new Finder();

        private static readonly Logger Log = new LoggerConfiguration()
#if DEBUG
                                             .MinimumLevel.Verbose()
#else
                                             .MinimumLevel.Warning()
#endif
                                             .WriteTo.Console()
                                             .CreateLogger();


        static void Main(string[] args)
        {
            if (args.Length != 0)
                PrintLn("Allowed numbers of parameters: 0\n" +
                        $"Number of parameters specified: {args.Length}\n" +
                        "Ignoring console parameters.");

            PrintHeading("Exercise Runner");
            PrintLn("Whenever you are prompted for a decision after a list you can type the" +
                    " text in front of the colon (':')"); // TODO or the beginning of the name

            while (true)
            {
                PrintHeading("Main Menu", HeadingLevel.H2);
                PrintLn("The following exercises are availiable:");
                PrintList(Finder.Exercises
                                .Select(e => $"{e.Number}: {e.Name}")
                                .ToArray());

                FlushIn();
                string input = ReadLn("Type the number or 'exit'");
                if (input.ToLower() == "exit")
                {
                    break;
                }

                var exercise = Finder.FindExerciseByNumber(input);
                if (exercise == null)
                {
                    Print("No exercise found!");
                    continue;
                }

                ExerciseMenu(exercise.Value);
            }
        }

        private static void ExerciseMenu(ExerciseWithInfo exercise)
        {
            while (true)
            {
                PrintHeading(
                    string.IsNullOrWhiteSpace(exercise.Name)
                        ? exercise.Exercise.Name
                        : exercise.Name,
                    HeadingLevel.H3
                );
                PrintLn(exercise.Description.IsWhitespace()
                    ? "(No description given)"
                    : exercise.Description
                );

                var tasks = Finder.FindTasksInExercise(exercise);
                PrintLn();
                PrintLn("The following tasks are availiable:");
                PrintList(tasks.Select(t => $"{t.Number}: {t.Name}").ToArray());

                FlushIn();
                string input = ReadLn("Type in the number or 'exit'");
                if (input.Trim().ToLower() == "exit")
                {
                    return;
                }

                var task = Finder.FindTaskByNumber(tasks, input);
                if (task == null)
                {
                    PrintLn("Could not find task with specified number!");
                    continue;
                }

                RunTask(exercise, task.Value);
            }
        }

        private static void RunTask(ExerciseWithInfo exercise, TaskWithInfo task)
        {
            PrintHeading(exercise.Name + " - "
                                       + (task.Name.IsWhitespace()
                                           ? task.Task.Name
                                           : task.Name),
                HeadingLevel.H4
            );
            PrintLn(task.Description.IsWhitespace() ? "(No description given)" : task.Description);

            var exerciseInstance = exercise.Exercise
                                           .GetConstructor(new Type[] { })
                                           .Invoke(new object[] { });
            task.Task.Invoke(exerciseInstance, new object[] { });
        }
    }
}