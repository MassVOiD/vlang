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
    }
}