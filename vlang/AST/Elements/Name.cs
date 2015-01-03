using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Name : ASTElement, IASTElement
    {
        private string Identifier;

        public Name(string name)
        {
            Identifier = name;
        }

        public Variable GetReference(ExecutionContext context)
        {
            return context.GetReference(Identifier);
        }

        public object GetValue(ExecutionContext context)
        {
            return context.GetValue(Identifier);
        }

        public override string ToJSON()
        {
            return String.Format("Name({0})", Identifier);
        }
    }
}