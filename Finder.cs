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

        public Finder()
        {
            Log.Debug("Exercises: {@exercises}", this.Exercises);
        }

        public ExerciseWithInfo? FindExerciseByNumber(string input)
        {
            var matches = this.Exercises.Where(e => e.Number == input).ToList();
            if (matches.Any())
            {
                return matches.First();
            }

            return null;
        }

        public static List<TaskWithInfo> FindTasksInExercise(ExerciseWithInfo exercise)
        {
            var methods = (
                from m in exercise.Exercise.GetMethods()
                select m
            ).ToList();
            Log.Debug("Availiable methods: {@m}", methods);

            var tasks = (
                from m in methods
                where m.IsDefined(typeof(TaskAttribute), false)
                select m
            ).ToList();
            Log.Debug("Availiable tasks: {@t}", tasks);

            return (
                from m in tasks
                let twi = new TaskWithInfo(
                    m,
                    m.GetCustomAttribute(typeof(TaskAttribute)) as TaskAttribute
                )
                orderby twi.Number
                select twi
            ).ToList();
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
                where t.IsDefined(typeof(ExerciseAttribute), true)
                select new ExerciseWithInfo(
                    t,
                    t.GetCustomAttribute(typeof(ExerciseAttribute)) as ExerciseAttribute)
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
    }
}