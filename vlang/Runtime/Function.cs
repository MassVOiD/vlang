using System;
using System.Collections.Generic;
using VLang.AST;

namespace VLang.Runtime
{
    internal class Function : ICallable
    {
        private Dictionary<int, List<string>> ArgumentNames;
        private Dictionary<int, int> Overloads;

        public Function(List<string> names, int body)
        {
            ArgumentNames = new Dictionary<int, List<string>>();
            ArgumentNames.Add(names.Count, names);
            Overloads = new Dictionary<int, int>();
            Overloads.Add(names.Count, body);
        }

        public void AddOverload(List<string> names, int body)
        {
            if (Overloads.ContainsKey(names.Count))
            {
                throw new Exception("Function already exists");
            }
            if (ArgumentNames.ContainsKey(names.Count))
            {
                throw new Exception("Function already exists");
            }
            ArgumentNames = new Dictionary<int, List<string>>();
            ArgumentNames.Add(names.Count, names);
            Overloads = new Dictionary<int, int>();
            Overloads.Add(names.Count, body);
        }

        public object Call(ExecutionContext context)
        {
            if (!Overloads.ContainsKey(0))
            {
                throw new Exception("Function not found");
            }
            ExecutionContext newContext = new ExecutionContext(context);
            return context.GetGroup(Overloads[0]).GetValue(newContext);
        }

        public object Call(ExecutionContext context, object[] args)
        {
            if (!Overloads.ContainsKey(args.Length))
            {
                throw new Exception("Function not found");
            }
            if (!ArgumentNames.ContainsKey(args.Length))
            {
                throw new Exception("Function not found");
            }
            ExecutionContext newContext = new ExecutionContext(context);
            for (int i = 0; i < args.Length; i++)
            {
                newContext.SetValue(ArgumentNames[args.Length][i], args[i]);
            }
            return context.GetGroup(Overloads[args.Length]).GetValue(newContext);
        }

        public bool IsVoid()
        {
            return false;
        }
    }
}