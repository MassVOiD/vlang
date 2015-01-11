using System;
using System.Collections.Generic;
using System.Linq;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class New : ASTElement, IASTElement
    {
        public List<IASTElement> Arguments;
        public IASTElement Name;

        public New(IASTElement name, List<IASTElement> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        public object GetValue(ExecutionContext context)
        {
            string name = Name.GetValue(context).ToString();
            var args = Arguments.Select<IASTElement, object>(a => a.GetValue(context)).ToArray();
            return context.InteropManager.CreateInstance(name, args);
        }

        public override string ToJSON()
        {
            return String.Format("new {0}({1})", Name.ToJSON(), String.Join(",", Arguments.Select<IASTElement, string>(a => a.ToJSON())));
        }
    }
}