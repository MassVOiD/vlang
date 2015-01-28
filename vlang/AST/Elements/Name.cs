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
    }
}