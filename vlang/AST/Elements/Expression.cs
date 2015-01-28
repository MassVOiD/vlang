using System;
using System.Collections.Generic;
using System.Linq;

namespace VLang.AST.Elements
{
    public class Expression : ASTElement, IASTElement
    {
        public List<IASTElement> List;

        public Stack<object> Stack = new Stack<object>();

        public Expression(List<IASTElement> list)
        {
            List = list;
        }

        string revertInfixNotation()
        {
            var stack = new Stack<IASTElement>();
            var sb = new System.Text.StringBuilder();
            foreach (var element in List)
            {
                if (element is Operator)
                {
                    int argc = ((Operator)element).GetArgumentsCount();
                    if (argc == 2 && stack.Count >= 2)
                    {
                        var v1 = stack.Count != 0 ? stack.Pop() : null;
                        var v2 = stack.Count != 0 ? stack.Pop() : null;
                        if (v1 is Value && v2 is Value)
                        {
                            stack.Push(new Value(((Operator)element).Execute(new dynamic[] { ((Value)v2).Val, ((Value)v1).Val })));
                        }
                        else
                        {
                            sb.Append("(" + v2.ToJSON());
                            sb.Append(element.ToJSON());
                            sb.Append(v1.ToJSON() + ")");
                        }
                    }
                    else if (argc == 1 && stack.Count >= 1)
                    {
                        var v1 = stack.Count != 0 ? stack.Pop() : null;
                        sb.Append(element.ToJSON());
                        sb.Append(v1.ToJSON());
                    }
                    //while(!(stack.Peek() is Operator)) sb.Append(stack.Pop().ToJSON());
                }
                else stack.Push(element);
            }
            while (stack.Count != 0) sb.Append(stack.Pop().ToJSON());
            return sb.ToString();
        }

        public override string ToJSON()
        {
            return String.Format("{0}", revertInfixNotation());
        }

        public class CLRReference
        {
            public object instance, field;
        }
    }
}