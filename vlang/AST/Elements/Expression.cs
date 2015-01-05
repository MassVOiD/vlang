using System;
using System.Collections.Generic;
using System.Linq;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    public class Expression : ASTElement, IASTElement
    {
        public List<IASTElement> List;

        public Expression(List<IASTElement> list)
        {
            List = list;
        }

        public Stack<object> Stack = new Stack<object>();

        public object GetValue(ExecutionContext context)
        {
            foreach (IASTElement element in List)
            {
                context.UpdateEvaluationStack(this);
                if (element is Operator)
                {
                    Operator op = element as Operator;
                    int argc = op.GetArgumentsCount();
                    List<object> args = new List<object>();
                    while (argc-- > 0) args.Insert(0, Stack.Pop());
                    Stack.Push(op.Execute(args.ToArray()));
                }
                else if (element.HasValue(context))
                {
                    object val = element.GetValue(context);
                    while (val is IASTElement) val = ((IASTElement)val).GetValue(context);
                    Stack.Push(val);
                }
                else
                {
                    object val = element.GetValue(context);
                    if (val != null) Stack.Push(val); // THIS IS NOT SO OBVIOUS!
                    while (val is IASTElement) val = ((IASTElement)val).GetValue(context);
                }
            }
            if (Stack.Count == 0) return null;
            var value = Stack.Pop();
            while (value is IASTElement) value = ((IASTElement)value).GetValue(context);
            Stack = new Stack<object>();
            return value;
        }

        public class CLRReference
        {
            public object instance, field;
        }

        public CLRReference GetCLRReference(ExecutionContext context)
        {
            foreach (IASTElement element in List)
            {
                //context.UpdateEvaluationStack(this);
                if (element is Operator)
                {
                    Operator op = element as Operator;
                    int argc = op.GetArgumentsCount();
                    List<object> args = new List<object>();
                    while (argc-- > 0) args.Insert(0, Stack.Pop());
                    Stack.Push(op.Execute(args.ToArray()));
                }
                else if (element.HasValue(context))
                {
                    if (element == List.Last() && element is Name)
                    {
                        object val = context.InteropManager.ExtractReference(Stack.Peek(), ((Name)element).Identifier);
                        Stack.Push(new CLRReference()
                        {
                            field = val, instance = Stack.Pop()
                        });
                    }
                    else
                    {
                        object val = element.GetValue(context);
                        while (val is IASTElement) val = ((IASTElement)val).GetValue(context);
                        Stack.Push(val);
                    }
                }
            }
            if (Stack.Count == 0) return null;
            var value = Stack.Pop();
            while (value is IASTElement) value = ((IASTElement)value).GetValue(context);
            Stack = new Stack<object>();
            if (value is CLRReference) return (CLRReference)value;
            else throw new Exception("Cannot find reference");
        }

        public override string ToJSON()
        {
            return String.Format("Expression{{{0}}}", String.Join(",", List.Select<IASTElement, string>(a => a.ToJSON())));
        }
    }
}