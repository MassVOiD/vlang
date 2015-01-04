using System;
using System.Collections.Generic;
using System.Linq;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Expression : ASTElement, IASTElement
    {
        private List<IASTElement> List;

        public Expression(List<IASTElement> list)
        {
            List = list;
        }

        public object GetValue(ExecutionContext context)
        {
            Stack<object> tempStack = new Stack<object>();
            foreach (IASTElement element in List)
            {
                if (element is Operator)
                {
                    Operator op = element as Operator;
                    int argc = op.GetArgumentsCount();
                    List<object> args = new List<object>();
                    while (argc-- > 0) args.Insert(0, tempStack.Pop());
                    tempStack.Push(op.Execute(args.ToArray()));
                }
                else if (element.HasValue(context))
                {
                    object val = element.GetValue(context);
                    while (val is IASTElement) val = ((IASTElement)val).GetValue(context);
                    tempStack.Push(val);
                }
                else
                {
                    object val = element.GetValue(context);
                    if (val != null) tempStack.Push(val); // THIS IS NOT SO OBVIOUS!
                    while (val is IASTElement) val = ((IASTElement)val).GetValue(context);
                }
            }
            if (tempStack.Count == 0) return null;
            var value = tempStack.Pop();
            while (value is IASTElement) value = ((IASTElement)value).GetValue(context);
            return value;
        }

        public override string ToJSON()
        {
            return String.Format("{0}", String.Join(",", List.Select<IASTElement, string>(a => a.ToJSON())));
        }
    }
}