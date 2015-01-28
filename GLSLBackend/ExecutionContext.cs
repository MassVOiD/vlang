using System;
using System.Collections.Generic;
using System.Linq;
using VLang;
using VLang.AST;
using VLang.AST.Elements;

namespace InterpreterBackend
{
    public class ExecutionContext
    {
        public InteropManager InteropManager;
        public ExecutionContext Root;
        private Dictionary<string, Variable> Fields;
        private Dictionary<int, ASTNode> Groups;
        private Stack<object> RealEvaluationStack;
        private List<ExecutionContext> SearchPath;

        public ExecutionContext(params ExecutionContext[] parents)
        {
            Fields = new Dictionary<string, Variable>();
            SearchPath = new List<ExecutionContext>();
            SearchPath.AddRange(parents);
            var tmp = SearchPath.FirstOrDefault(a => a.InteropManager != null);
            if (tmp != null) InteropManager = tmp.InteropManager;
            if (parents.Length == 0) Root = this;
            else
            {
                Root = parents.First(a => a.Root != null);
            }
        }

        public Stack<object> EvaluationStack
        {
            get
            {
                return Root.RealEvaluationStack;
            }
        }

        public bool Exists(string name)
        {
            var value = Fields.Where(a => a.Key == name);
            if (value == null || value.Count() == 0)
            {
                foreach (var context in SearchPath)
                {
                    var tmp = context.GetValue(name);
                    if (tmp != null) return true;
                }
            }
            return value.Count() != 0;
        }

        public ASTNode GetGroup(int gid)
        {
            var reference = this;
            if (reference.Groups != null && reference.Groups.ContainsKey(gid))
            {
                return reference.Groups[gid];
            }
            while (reference.Groups == null && reference.SearchPath.Count > 0)
            {
                foreach (var e in reference.SearchPath)
                {
                    var res = e.GetGroup(gid);
                    if (res != null) return res;
                }
            }
            return null;
        }

        public Variable GetReference(string name)
        {
            var value = Fields.Where(a => a.Key == name);
            if (value == null || value.Count() == 0)
            {
                foreach (var context in SearchPath)
                {
                    var tmp = context.GetReference(name);
                    if (tmp != null) return tmp;
                }
            }
            return value.First().Value;
        }

        public Variable CreateField(string name, Type type, object value)
        {
            var v = new Variable(type, value);
            Fields.Add(name, v);
            return v;
        }

        public object GetValue(string name, bool dontThrowIfNotFound = false, object defaultValue = null)
        {
            var value = Fields.ContainsKey(name) ? Fields[name] : (object)new NotFoundFieldObjectStub();
            if (value is NotFoundFieldObjectStub)
            {
                foreach (var context in SearchPath)
                {
                    var tmp = context.GetValue(name);
                    if (tmp != null) return tmp;
                }
            }
            if (value is NotFoundFieldObjectStub && dontThrowIfNotFound) return defaultValue;
            else if (!dontThrowIfNotFound && value is NotFoundFieldObjectStub) throw new Exception("Field not found");
            return ((Variable)value).Value;
        }

        public void SetGroups(Dictionary<int, ASTNode> g)
        {
            Groups = g;
        }

        public void SetInteropManager(InteropManager im)
        {
            InteropManager = im;
        }

        public void SetValue(string name, object value)
        {
            if (Fields.ContainsKey(name))
            {
                Fields[name].Value = value;
            }
            else
            {
                throw new MissingFieldException(name);
            }
        }

        public void UpdateEvaluationStack(Expression expression)
        {
            if (RealEvaluationStack == null) RealEvaluationStack = new Stack<object>();
            var tmp = expression.Stack.ToList();
            tmp.Reverse();
            foreach (var obj in tmp) Root.EvaluationStack.Push(obj);
        }

        private class NotFoundFieldObjectStub { }

        public object Execute(IASTElement leaf, bool createBranch = false)
        {
            if(createBranch)
                return new ExecutionContext(this).Execute(leaf);

            if(leaf is Expression)
            {
                var expression = leaf as Expression;
                foreach(IASTElement element in expression.List)
                {
                    UpdateEvaluationStack(expression);
                    if(element is Operator)
                    {
                        Operator op = element as Operator;
                        int argc = op.GetArgumentsCount();
                        List<object> args = new List<object>();
                        while(argc-- > 0)
                            args.Insert(0, expression.Stack.Pop());
                        expression.Stack.Push(op.Execute(args.ToArray()));
                    }
                    else
                    {
                        object val = Execute(element);
                        while(val is IASTElement)
                        {
                            val = Execute((IASTElement)val);
                        }
                        expression.Stack.Push(val);
                    }
                }
                if(expression.Stack.Count == 0)
                    return null;
                var value = expression.Stack.Pop();
                while(value is IASTElement)
                {
                    value = Execute((IASTElement)value);
                }
                expression.Stack = new Stack<object>();
                return value;
            }

            if(leaf is Assignment)
            {
                var assignment = leaf as Assignment;
                object val = Execute(assignment.Value);
                Variable target = GetReference((string)Execute(assignment.Target));
                target.Value = val;
                return val;
            }

            if(leaf is VariableDeclaration)
            {
                var vardef = leaf as VariableDeclaration;
                object val = Execute(vardef.Value);
                Variable target = CreateField((string)Execute(vardef.Target), InteropManager.GetTypeByName(vardef.TypeName), val);
                return val;
            }

            if(leaf is Conditional)
            {
                var cond = leaf as Conditional;
                object expraw = Execute(cond.Condition);
                bool? exp = null;
                if(expraw is bool)
                    exp = (bool)expraw;
                else
                    throw new Exception("Not boolean in conditional expression");
                return exp.Value ? Execute(cond.IfNode, true) : (cond.ElseNode != null ? Execute(cond.ElseNode, true) : null);
            }

            if(leaf is ASTNode)
            {
                (leaf as ASTNode).ForEach(a => Execute(a));
                return EvaluationStack.Count > 0 ? EvaluationStack.Pop() : null;
            }

            if(leaf is Operator)
            {
                throw new Exception("Operator cannot be called directly");
            }
            if(leaf is Value)
            {
                return (leaf as Value).Val;
            }
            if(leaf is Name)
            {
                var name = leaf as Name;
                object res = GetValue(name.Identifier);
                if(res != null)
                    return res;
                res = InteropManager.GetTypeByName(name.Identifier);
                if(res != null)
                    return res;
                if(InteropManager.DoesUseNamesace(name.Identifier))
                    return new InteropManager.NamespaceInfo(name.Identifier);
                if(EvaluationStack == null || EvaluationStack.Count == 0)
                {
                    SetValue(name.Identifier, null);
                    return GetReference(name.Identifier);
                }
                res = InteropManager.Extract(EvaluationStack.Peek(), name.Identifier);
                return res;
            }

            if(leaf is FunctionCall)
            {
                var func = leaf as FunctionCall;
                object searchResult = Execute(func.Func);
                string Name = searchResult.ToString();
                //if (Name.Contains(".")) return ReflectionWiseCall(Name, context);
                object function = searchResult is ICallable ? searchResult : GetValue(Name);
                if(function is ICallable)
                {
                    if(func.Arguments.Count == 0)
                        return ((ICallable)function).Call(this);
                    else
                        return ((ICallable)function).Call(this, func.Arguments.Select<IASTElement, object>(a => Execute(a)).ToArray());
                }
                else
                {
                    throw new Exception("Function not callable");
                }
            }

            return null;
        }
    }
}