using System;

namespace VLang.AST.Elements
{
    public class Name : ASTElement, IASTElement
    {
        public string Identifier;

        public Name(string name)
        {
            Identifier = name;
        }

        public override string ToJSON()
        {
            return String.Format("{0}", Identifier);
        }
    }
}