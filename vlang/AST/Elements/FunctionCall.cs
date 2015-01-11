using System;
using System.Collections.Generic;
using System.Linq;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class FunctionCall : ASTElement, IASTElement
    {
        public List<IASTElement> Arguments;
        public IASTElement Func;

        public FunctionCall(IASTElement func, params IASTElement[] arguments)
        {
            Func = func;
            Arguments = arguments.ToList();
        }

        public object GetValue(ExecutionContext context)
        {
            object searchResult = Func.GetValue(context);
            string Name = Func.GetValue(context).ToString();
            //if (Name.Contains(".")) return ReflectionWiseCall(Name, context);
            object function = searchResult is ICallable ? searchResult : context.GetValue(Name, true);
            if (function == null) return ReflectionWiseCall(Name, context);
            if (function is ICallable)
            {
                if (Arguments.Count == 0) return ((ICallable)function).Call(context);
                else return ((ICallable)function).Call(context, Arguments.Select<IASTElement, object>(a => a.GetValue(context)).ToArray());
            }
            else
            {
                throw new Exception("Function not callable");
            }
        }

        public override bool HasValue(ExecutionContext context)
        {
            return true; // not so correct
        }

        public override string ToJSON()
        {
            string Name = Func.ToJSON();
            return String.Format("{0}({1})", Name, String.Join(",", Arguments.Select<IASTElement, string>(a => a.ToJSON())));
        }

        private object ReflectionWiseCall(string name, ExecutionContext context)
        {
            object obj = context.EvaluationStack.Pop();
            if (Arguments.Count == 0)
            {
                if (obj is Type) return new ReflectionMethod(((Type)obj), name).Call(context);
                else return new ReflectionMethod(context.EvaluationStack.Pop(), name).Call(context);
            }
            else
            {
                if (obj is Type) return new ReflectionMethod(((Type)obj), name).Call(context);
                else return new ReflectionMethod(context.EvaluationStack.Pop(), name).Call(context);
            }
        }

        private bool ReflectionWiseVoidCheck(string name, ExecutionContext context)
        {
            if (name.Contains('.'))
            {
                string[] parts = name.Split('.');
                object test = context.GetValue(parts[0], true);
                if (test != null)
                {
                    return new ReflectionMethod(test, parts[1]).IsVoid();
                }
                else
                {
                    Type val = context.InteropManager.GetTypeByName(parts[0]);
                    return new ReflectionMethod(val, parts[1]).IsVoid();
                }
            }
            throw new Exception("Method not found");
        }
    }
}