using System;

namespace VLang.AST.Elements
{
    public class Conditional : ASTElement, IASTElement
    {
        public IASTElement Condition;
        public ASTNode IfNode, ElseNode;

        public Conditional(IASTElement condition, ASTNode trueBranch, ASTNode falseBranch = null)
        {
            Condition = condition;
            IfNode = trueBranch;
            ElseNode = falseBranch;
        }
        
        public override string ToJSON()
        {
            if (ElseNode != null)
            {
                return String.Format("if({0}){{{1}}}else{{{2}}}", Condition.ToJSON(), IfNode.ToJSON(), ElseNode.ToJSON());
            }
            else
            {
                return String.Format("if({0}){{{1}}}", Condition.ToJSON(), IfNode.ToJSON());
            }
        }
    }
}