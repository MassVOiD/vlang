using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Using : ASTElement, IASTElement
    {
        private string Name;

        public Using(string name)
        {
            Name = name;
        }

        public object GetValue(ExecutionContext c)
        {
            c.InteropManager.UseNamespace(Name);
            return null;
        }

        public override string ToJSON()
        {
            return String.Format("Using({0})", Name);
        }
    }
}