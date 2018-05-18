using System;
using System.Reflection;
using LibConsole;

namespace LibExerciseRunner
{
    public struct ExerciseWithInfo
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

    public struct TaskWithInfo
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