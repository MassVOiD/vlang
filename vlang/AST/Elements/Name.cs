using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Name : ASTElement, IASTElement
    {
        public string Identifier;

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
            object res = context.GetValue(Identifier, true);
            if (res != null) return res;
            res = context.InteropManager.GetTypeByName(Identifier);
            if (res != null) return res;
            if (context.InteropManager.DoesUseNamesace(Identifier)) return new InteropManager.NamespaceInfo(Identifier);
            if (context.EvaluationStack == null || context.EvaluationStack.Count == 0)
            {
                context.SetValue(Identifier, null);
                return context.GetReference(Identifier);
            }
            res = context.InteropManager.Extract(context.EvaluationStack.Peek(), Identifier);
            return res;
        }

        public override string ToJSON()
        {
            return String.Format("Name({0})", Identifier);
        }
    }
}