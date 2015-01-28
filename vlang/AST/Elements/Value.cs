using System;

namespace VLang.AST.Elements
{
    public class Value : ASTElement, IASTElement
    {
        public object Val;

        public Value(object value)
        {
            Val = value;
        }
    }
}