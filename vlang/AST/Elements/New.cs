using System;
using System.Collections.Generic;
using System.Linq;

namespace VLang.AST.Elements
{
    public class New : ASTElement, IASTElement
    {
        public List<IASTElement> Arguments;
        public IASTElement Name;

        public New(IASTElement name, List<IASTElement> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        public override string ToJSON()
        {
            return String.Format("new {0}({1})", Name.ToJSON(), String.Join(",", Arguments.Select<IASTElement, string>(a => a.ToJSON())));
        }
    }
}