using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Value : ASTElement, IASTElement
    {
        private object Val;

        public Value(object value)
        {
            Val = value;
        }

        public object GetValue(ExecutionContext context)
        {
            return Val;
        }

        public override string ToJSON()
        {
            return String.Format("Value({0})", Val.ToString());
        }
    }
}