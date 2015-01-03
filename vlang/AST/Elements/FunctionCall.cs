using System;
using System.Collections.Generic;
using System.Linq;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class FunctionCall : ASTElement, IASTElement
    {
        private List<Expression> Arguments;
        private string Name;

        public FunctionCall(string name, params Expression[] arguments)
        {
            Name = name;
            Arguments = arguments.ToList();
        }

        public object GetValue(ExecutionContext context)
        {
            object function = context.GetValue(Name);
            if (function is ICallable) return ((ICallable)function).Call(context, Arguments.Select<Expression, object>(a => a.GetValue(context)).ToArray());
            else throw new Exception("Function not callable");
        }

        public override bool HasValue(ExecutionContext context)
        {
            object function = context.GetValue(Name);
            if (function is ICallable) return !((ICallable)function).IsVoid();
            else throw new Exception("Function not callable");
        }

        public override string ToJSON()
        {
            return String.Format("FunctionCall({0}{{{1}}})", Name, String.Join(",", Arguments.Select<Expression, string>(a => a.ToJSON())));
        }
    }
}