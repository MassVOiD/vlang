using System;
using System.Collections.Generic;
using System.Linq;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class FunctionCall : ASTElement, IASTElement
    {
        private List<Expression> Arguments;
        private Expression Func;

        public FunctionCall(Expression func, params Expression[] arguments)
        {
            Func = func;
            Arguments = arguments.ToList();
        }

        object ReflectionWiseCall(string name, ExecutionContext context)
        {
            object obj = context.EvaluationStack.Pop();
            if (obj is Type) return new ReflectionMethod(((Type)obj), name).Call(context, Arguments.Select<Expression, object>(a => a.GetValue(context)).ToArray());
            else return new ReflectionMethod(context.EvaluationStack.Pop(), name).Call(context, Arguments.Select<Expression, object>(a => a.GetValue(context)).ToArray());
        }

        bool ReflectionWiseVoidCheck(string name, ExecutionContext context)
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

        public object GetValue(ExecutionContext context)
        {
            object searchResult = Func.GetValue(context);
            string Name = Func.GetValue(context).ToString();
            //if (Name.Contains(".")) return ReflectionWiseCall(Name, context);
            object function = searchResult is ICallable ? searchResult : context.GetValue(Name, true);
            if (function == null) return ReflectionWiseCall(Name, context);
            if (function is ICallable) return ((ICallable)function).Call(context, Arguments.Select<Expression, object>(a => a.GetValue(context)).ToArray());
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
            return String.Format("FunctionCall({0}{{{1}}})", Name, String.Join(",", Arguments.Select<Expression, string>(a => a.ToJSON())));
        }
    }
}