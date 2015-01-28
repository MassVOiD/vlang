using System;
using System.Collections.Generic;
using System.Linq;

namespace VLang.AST.Elements
{
    public class FunctionCall : ASTElement, IASTElement
    {
        public List<IASTElement> Arguments;
        public IASTElement Func;

        public FunctionCall(IASTElement func, params IASTElement[] arguments)
        {
            Func = func;
            Arguments = arguments.ToList();
        }

        public override string ToJSON()
        {
            string Name = Func.ToJSON();
            return String.Format("{0}({1})", Name, String.Join(",", Arguments.Select<IASTElement, string>(a => a.ToJSON())));
        }

    }
}