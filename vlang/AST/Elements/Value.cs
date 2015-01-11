using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Value : ASTElement, IASTElement
    {
        public object Val;

        public Value(object value)
        {
            Val = value;
        }

        public object GetValue(ExecutionContext context)
        {
            return Val;
        }
        public object GetValue()
        {
            return Val;
        }

        public override string ToJSON()
        {
            if(Val is string) return String.Format("'{0}'", Val.ToString());
            else return String.Format("{0}", Val.ToString());
        }
    }
}