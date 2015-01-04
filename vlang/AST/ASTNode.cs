using System;
using System.Collections.Generic;
using System.Linq;
using VLang.Runtime;

namespace VLang.AST
{
    public class ASTNode : List<IASTElement>, IASTElement
    {

        public Dictionary<int, ASTNode> Groups;

        public void SetGroups(Dictionary<int, ASTNode> g)
        {
            if (Groups != null) throw new Exception("Groups can be set only once");
            Groups = g;
        }

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

        public List<IASTElement> GetAllNodesFlat()
        {
            List<IASTElement> leafs = this.Where(a => a is IASTElement && !(a is Elements.Expression)).ToList();
            List<IASTElement> branches = this.Where(a => a is Elements.Expression || a is ASTNode).ToList();
            foreach (var branch in branches)
            {
                if (branch is ASTNode) leafs.AddRange(((ASTNode)branch).GetAllNodesFlat());
                else leafs.AddRange(((Elements.Expression)branch).List);
            }
            return leafs;
        }

        public string ToJSON()
        {
            return String.Format("ASTNode{{{0};}};", String.Join(";", this.Select<IASTElement, string>(a => a.ToJSON())));
        }
    }
}