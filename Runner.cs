using System;
using System.Linq;
using LibConsole;
using static LibConsole.PrettyPrint;
using static LibConsole.PrettyRead;

namespace LibExerciseRunner
{
    public static class Runner
    {
        private static readonly Finder Finder = new Finder();

        public static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                PrintLn("Allowed numbers of parameters: 0\n" +
                        "Number of parameters specified: " + args.Length +
                        "\nIgnoring console parameters.");
            }

            PrintHeading("Exercise Runner");
            PrintLn("Often you have to choose an option out of a list. " +
                    "In those cases you have to type in the number of the item.");

            while (MainMenu())
            {
            }
        }

        /// <returns>false if the user signaled that he wants to exit</returns>
        private static bool MainMenu()
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
                return false;
            }

            var exercise = Finder.FindExerciseByNumber(input);
            if (exercise == null)
            {
                PrintLn($"No exercise with number '{input}' found!");
                return true;
            }

            while (ExerciseMenu(exercise.Value)) ;
            return true;
        }

        /// <returns>false if the user signaled that he wants to exit</returns>
        private static bool ExerciseMenu(ExerciseWithInfo exercise)
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

            var tasks = Finder.GetTasksInExercise(exercise);
            PrintLn();
            PrintLn("The following tasks are availiable:");
            PrintList(tasks.Select(t => $"{t.Number}: {t.Name}").ToArray());

            FlushIn();
            string input = ReadLn("Type in the number or 'exit'");
            if (input.Trim().ToLower() == "exit")
            {
                return false;
            }

            var task = Finder.FindTaskByNumber(tasks, input);
            if (task == null)
            {
                PrintLn("Could not find task with specified number!");
                return true;
            }

            RunTask(exercise, task.Value);
            return true;
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