using System;
using System.Reflection;

namespace InterpreterBackend
{
    public class Delegation : ICallable
    {
        private Delegate callback;

        public Delegation(Delegate callback)
        {
            this.callback = callback;
        }

        public object Call(ExecutionContext context)
        {
            return Call(new object[0]);
        }

        public object Call(ExecutionContext context, object[] args)
        {
            return Call(args);
        }

        public bool IsVoid()
        {
            return callback.GetMethodInfo().ReturnType == typeof(void);
        }

        private object Call(object[] arguments)
        {
            return callback.DynamicInvoke(arguments);
        }
    }
}