using System;
using System.Collections.Generic;
using System.Linq;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class New : ASTElement, IASTElement
    {
        private Expression Name;
        private List<Expression> Arguments;

        public New(Expression name, List<Expression> arguments)
        {
            Name = name;
            Arguments = arguments;
        }
        
        public object GetValue(ExecutionContext context)
        {
            string name = Name.GetValue(context).ToString();
            var args = Arguments.Select<Expression, object>(a => a.GetValue(context)).ToArray();
            return context.InteropManager.CreateInstance(name, args);
        }

        public override string ToJSON()
        {
            return String.Format("New({0}({1}))", Name, String.Join(",", Arguments.Select<Expression, string>(a => a.ToJSON())));
        }
    }
}