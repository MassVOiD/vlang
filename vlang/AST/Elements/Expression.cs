using System;
using System.Collections.Generic;
using System.Linq;
using VLang.Runtime;

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

        public CLRReference GetCLRReference(ExecutionContext context)
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
                    if (element == List.Last() && element is Name)
                    {
                        object val = context.InteropManager.ExtractReference(Stack.Peek(), ((Name)element).Identifier);
                        Stack.Push(new CLRReference()
                        {
                            field = val,
                            instance = Stack.Pop()
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
                            stack.Push(new Value(((Operator)element).Execute(new dynamic[] { ((Value)v2).GetValue(), ((Value)v1).GetValue() })));
                        }
                        else
                        {
                            sb.Append(v2.ToJSON());
                            sb.Append(element.ToJSON());
                            sb.Append(v1.ToJSON());
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
            return String.Format("{0}", String.Join(" >> ", List.Select<IASTElement, string>(a => a.ToJSON())));
        }

        public class CLRReference
        {
            public object instance, field;
        }
    }
}