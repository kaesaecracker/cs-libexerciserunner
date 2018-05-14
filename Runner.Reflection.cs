using System.Runtime.InteropServices.ComTypes;

namespace LibConsoleRunner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Library;

    public partial class Program
    {
        private static List<ExerciseWithInfo> GetExercises()
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

        private static List<TaskWithInfo> GetTasks(ExerciseWithInfo exercise)
            => GetTasks(new List<ExerciseWithInfo> {exercise});

        private static List<TaskWithInfo> GetTasks(List<ExerciseWithInfo> exercises)
        {
            var methods = (
                from e in exercises
                from m in e.Exercise.GetMethods()
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
    }
}