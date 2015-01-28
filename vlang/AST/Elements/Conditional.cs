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
    }
}