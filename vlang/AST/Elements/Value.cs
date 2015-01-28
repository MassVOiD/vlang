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

        public override string ToJSON()
        {
            if(Val is string)
                return String.Format("'{0}'", Val.ToString());
            if(Val is float)
                return String.Format("{0}", ((float)Val).ToString().Replace(',', '.'));
            if(Val is double)
                return String.Format("{0}", ((double)Val).ToString().Replace(',', '.'));
            else return String.Format("{0}", Val.ToString());
        }
    }
}