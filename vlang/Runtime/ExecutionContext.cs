using System.Collections.Generic;
using System.Linq;
using VLang.AST;

namespace VLang.Runtime
{
    public class ExecutionContext
    {
        private Dictionary<string, Variable> Fields;
        private List<ExecutionContext> SearchPath;
        private Dictionary<int, ASTNode> Groups;
        public InteropManager InteropManager;

        public ExecutionContext(params ExecutionContext[] parents)
        {
            Fields = new Dictionary<string, Variable>();
            SearchPath = new List<ExecutionContext>();
            SearchPath.AddRange(parents);
            if (InteropManager == null)
            {
                var tmp = SearchPath.FirstOrDefault(a => a.InteropManager != null);
                if (tmp != null) InteropManager = tmp.InteropManager;
            }
        }

        public void SetInteropManager(InteropManager im)
        {
            InteropManager = im;
        }

        public void SetGroups(Dictionary<int, ASTNode> g)
        {
            Groups = g;
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

        public object GetValue(string name)
        {
            var value = Fields.Where(a => a.Key == name);
            if (value == null || value.Count() == 0)
            {
                foreach (var context in SearchPath)
                {
                    var tmp = context.GetValue(name);
                    if (tmp != null) return tmp;
                }
            }
            return value.First().Value.Value;
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
    }
}