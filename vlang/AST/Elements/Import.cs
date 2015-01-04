using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Import : ASTElement, IASTElement
    {
        private string Name;

        public Import(string name)
        {
            Name = name;
        }

        public object GetValue(ExecutionContext c)
        {
            c.InteropManager.ImportAssembly(Name);
            return null;
        }

        public override string ToJSON()
        {
            return String.Format("Import({0})", Name);
        }
    }
}