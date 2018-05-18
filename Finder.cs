using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LibConsole;
using Serilog;
using static LibConsole.PrettyPrint;
using static LibConsole.PrettyRead;

namespace LibExerciseRunner
{
    public class Finder
    {
        public List<ExerciseWithInfo> Exercises { get; } = LoadExercises();

        public ExerciseWithInfo? FindExerciseByNumber(string input)
        {
            var matches = this.Exercises.Where(e => e.Number == input).ToList();
            if (matches.Any())
            {
                return matches.First();
            }

            return null;
        }

        public static List<TaskWithInfo> GetTasksInExercise(ExerciseWithInfo exercise)
        {
            return (
                from m in exercise.Exercise.GetMethods()
                where m.IsDefined(typeof(TaskAttribute), false)
                let twi = new TaskWithInfo(
                    m,
                    m.GetCustomAttribute(typeof(TaskAttribute)) as TaskAttribute
                )
                orderby twi.Number
                select twi
            ).ToList();
        }

        public TaskWithInfo? FindTaskByNumber(List<TaskWithInfo> tasks, string input)
        {
            var matches = tasks.Where(t => t.Number == input).ToList();
            if (matches.Any())
            {
                return matches.First();
            }

            return null;
        }

        private static List<ExerciseWithInfo> LoadExercises()
        {
            string executingDll = Assembly.GetExecutingAssembly().Location;
            string ownPath = Path.GetDirectoryName(executingDll);
            var dllFiles = (
                from d in Directory.EnumerateFiles(ownPath)
                where d.EndsWith("dll")
                      && d != executingDll
                select d
            ).ToArray();

            Log.Debug("Own path: {p}", ownPath);
            Log.Debug("DLL files to load: {@dll}", dllFiles.Select(Path.GetFileName));

            Log.Information("Loading DLL files in current directory");
            foreach (string dll in dllFiles)
            {
                try
                {
                    Assembly.LoadFile(dll);
                }
                catch (Exception ex)
                {
                    if (ex is FileLoadException || ex is BadImageFormatException)
                    {
                        Log.Warning("Could not load a dll file: {@exMsg}", ex.Message.Trim());
                        Log.Debug("{@ex}", ex);
                    }
                    else throw;
                }
            }

            var loadedAssemblys = (
                from a in AppDomain.CurrentDomain.GetAssemblies()
                where !a.FullName.StartsWith("System")
                      && !a.FullName.StartsWith("Serilog")
                select a
            ).ToList();
            Log.Debug("Assemblys loaded: {@a}", loadedAssemblys.Select(a => a.GetName().Name));

            return (
                from a in loadedAssemblys
                from t in a.GetTypes()
                where t.IsClass
                      && t.IsDefined(typeof(ExerciseAttribute), true)
                let ewi = new ExerciseWithInfo(
                    t,
                    t.GetCustomAttribute(typeof(ExerciseAttribute)) as ExerciseAttribute
                )
                orderby ewi.Number
                select ewi
            ).ToList();
        }
    }
}