using System;
using System.Collections.Generic;
using System.Linq;

namespace VLang.AST.Elements
{
    public class Expression : ASTElement, IASTElement
    {
        public List<IASTElement> List;

        public Stack<object> Stack = new Stack<object>();

        public Expression(List<IASTElement> list)
        {
            List = list;
        }


        public class CLRReference
        {
            public object instance, field;
        }
    }
}