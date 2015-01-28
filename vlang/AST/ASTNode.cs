using System;
using System.Collections.Generic;
using System.Linq;
using VLang.AST.Elements;

namespace VLang.AST
{
    public class ASTNode : List<IASTElement>, IASTElement
    {
        public Dictionary<int, ASTNode> Groups;

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

        public void Optimize()
        {
            for (int i = 0; i < this.Count; i++)
            {
                var branch = this[i];
                if (branch is ASTNode) ((ASTNode)branch).Optimize();
                else if (branch is Expression)
                {
                    Expression expr = branch as Expression;
                    if (expr.List.Count == 0)
                    {
                        this.RemoveAt(i--);
                    }
                    else if (expr.List.Count == 1)
                    {
                        while (expr != null)
                        {
                            this.RemoveAt(i);
                            this.Insert(i, expr.List.First());
                            expr = expr.List.First() as Expression;
                        }
                    }
                }
            }
            // there is possibility to remove dead code by cutting Group elements not existing in main AST branch.
            // It will break callbacks and reflection.
            foreach (var group in Groups) group.Value.Optimize();
        }

        public void SetGroups(Dictionary<int, ASTNode> g)
        {
            if (Groups != null) throw new Exception("Groups can be set only once");
            Groups = g;
        }
    }
}