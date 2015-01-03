using System;
using System.Collections.Generic;
using System.Linq;
using VLang.Runtime;

namespace VLang.AST
{
    public class ASTNode : List<IASTElement>, IASTElement
    {
        public object GetValue(ExecutionContext context)
        {
            object result = null;
            var newContext = new ExecutionContext(context);
            foreach (IASTElement element in this)
            {
                var tmp = element.GetValue(context);
                if (tmp != null) result = tmp;
                if (element is Elements.Return) break;
            }
            return result;
        }

        public bool HasValue(ExecutionContext context)
        {
            return true;
        }

        public string ToJSON()
        {
            return String.Format("ASTNode{{{0};}};", String.Join(";", this.Select<IASTElement, string>(a => a.ToJSON())));
        }
    }
}