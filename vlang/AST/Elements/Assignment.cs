using System;
using System.Collections.Generic;

namespace VLang.AST.Elements
{
    public class Assignment : ASTElement, IASTElement
    {
        public IASTElement Target;
        public IASTElement Value;

        public Assignment(IASTElement target, IASTElement value)
        {
            Target = target;
            Value = value;
        }

    }
}