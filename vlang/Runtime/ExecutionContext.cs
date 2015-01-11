using System;
using System.Collections.Generic;
using System.Linq;
using VLang.AST;

namespace VLang.Runtime
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
            if (value.Count() == 0)
            {
                var v = new Variable(null);
                Fields.Add(name, v);
                return v;
            }
            return value.First().Value;
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
                Fields[name] = new Variable(value);
            }
            else
            {
                Fields.Add(name, new Variable(value));
            }
        }

        public void UpdateEvaluationStack(AST.Elements.Expression expression)
        {
            if (RealEvaluationStack == null) RealEvaluationStack = new Stack<object>();
            var tmp = expression.Stack.ToList();
            tmp.Reverse();
            foreach (var obj in tmp) Root.EvaluationStack.Push(obj);
        }

        private class NotFoundFieldObjectStub { }
    }
}