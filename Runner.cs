using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Library;

namespace LibExerciseRunner
{
    using Serilog;
    using Serilog.Core;

    public partial class Program
    {
        private static readonly Logger Log = new LoggerConfiguration()
                                             .MinimumLevel.Verbose()
                                             .WriteTo.Console(
#if !DEBUG
                                                LogEventLevel.Warning
#endif
                                             ).WriteTo.File(
                                                 path: "logs/runner.log"
                                             ).CreateLogger();

        private static List<ExerciseWithInfo> _exercises = null;

        static void Main(string[] args)
        {
            _exercises = GetExercises();
            Log.Debug("Exercises: {@exercises}", _exercises);

            Console.Write("AlgoExercises Runner\n" +
                          "Whenever you are prompted for a decision after a list you can type the" +
                          " text in front of the colon (':')"); // TODO or the beginning of the name

            switch (args.Length)
            {
                case 1:
                    TaskAssistant(args[0]);
                    break;
                case 2:
                    TaskAssistant(args[0], args[1]);
                    break;

                default:
                    if (args.Length != 0)
                        Console.WriteLine("Allowed numbers of parameters: [0, 1, 2]\n" +
                                          $"Number of parameters specified: {args.Length}\n" +
                                          "Ignoring console parameters.");
                    TaskAssistant();
                    break;
            }
        }

        private static void TaskAssistant(string exerciseNumber)
        {
        }

        private static void TaskAssistant(string exerciseNumber, string taskNumber)
        {
        }

        private static void TaskAssistant()
        {
            Console.WriteLine("The following tasks are availiable:");
            foreach (var exercise in _exercises)
            {
                Console.WriteLine($"#{exercise.Number}: {exercise.Name}");
            }
        }

        private struct ExerciseWithInfo
        {
            public Type Exercise { get; }

            public string Number => this._attribute.Number;
            public string Name => this._attribute.Name;
            public string Description => this._attribute.Description;

            private readonly ExerciseAttribute _attribute;

            public ExerciseWithInfo(Type exercise, ExerciseAttribute attribute)
            {
                this.Exercise = exercise;
                this._attribute = attribute;
            }
        }

        private struct TaskWithInfo
        {
            public MethodInfo Task { get; }

            public string Number => this._attribute.Number;
            public string Name => this._attribute.Name;
            public string Description => this._attribute.Description;

            private readonly TaskAttribute _attribute;

            public TaskWithInfo(MethodInfo task, TaskAttribute attribute)
            {
                this.Task = task;
                this._attribute = attribute;
            }
        }
    }
}