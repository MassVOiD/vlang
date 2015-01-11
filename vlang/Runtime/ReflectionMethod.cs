using System;
using System.Linq;
using System.Reflection;

namespace VLang.Runtime
{
    public class ReflectionMethod : ICallable
    {
        private MethodInfo[] callback;
        private object reference;

        public ReflectionMethod(Object reference, string name)
        {
            var callback = reference.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Where(a => a.Name == name).ToArray();
            if (callback.Count() == 0) throw new Exception("Zero callbacks");
            this.callback = callback;
            this.reference = reference;
        }

        public ReflectionMethod(Type type, string name)
        {
            var callback = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(a => a.Name == name).ToArray();
            if (callback.Count() == 0) throw new Exception("Zero callbacks");
            this.callback = callback;
            this.reference = null;
        }

        public ReflectionMethod(Object reference, MethodInfo[] callback)
        {
            if (callback.Count() == 0) throw new Exception("Zero callbacks");
            this.callback = callback;
            this.reference = reference;
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
            return callback.First().ReturnType == typeof(void);
        }

        public override String ToString()
        {
            return this.callback[0].Name;
        }

        private object Call(object[] arguments)
        {
            if (arguments.Length == 0)
            {
                return callback.First(a => a.GetParameters().Length == 0).Invoke(this.reference, arguments.ToArray());
            }
            foreach (MethodInfo m in callback)
            {
                ParameterInfo[] param = m.GetParameters();
                if (param.Length != arguments.Length) continue;
                int iterator = 0, score = 0;
                foreach (ParameterInfo p in param)
                {
                    if (p.ParameterType == arguments[iterator].GetType()) score++;
                    else if (p.ParameterType == arguments[iterator].GetType() || p.ParameterType == typeof(object)) score++;
                    else
                    {
                        Type baseType = arguments[iterator].GetType().BaseType;
                        while (baseType != typeof(object) && baseType != baseType.BaseType)
                        {
                            try
                            {
                                if (p.ParameterType == baseType)
                                {
                                    score++;
                                    baseType = baseType.BaseType;
                                    continue;
                                }
                                baseType = baseType.BaseType;
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }
                    iterator++;
                }
                if (score == arguments.Length) return m.Invoke(this.reference, arguments.ToArray());
            }
            throw new Exception("Method " + this.callback[0].Name + " with provided arguments not found in internal function list");
        }
    }
}