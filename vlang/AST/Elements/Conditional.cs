using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Conditional : ASTElement, IASTElement
    {
        private Expression Condition;
        private ASTNode IfNode, ElseNode;

        public Conditional(Expression condition, ASTNode trueBranch, ASTNode falseBranch = null)
        {
            Condition = condition;
            IfNode = trueBranch;
            ElseNode = falseBranch;
        }

        public object GetValue(ExecutionContext context)
        {
            object expraw = Condition.GetValue(context);
            bool? exp = null;
            if (expraw is bool) exp = (bool)expraw;
            else throw new Exception("Not boolean in conditional expression");
            return exp.Value ? IfNode.GetValue(context) : (ElseNode != null ? ElseNode.GetValue(context) : null);
        }

        public override string ToJSON()
        {
            return String.Format("Conditional({0})({{{1}}})({{{2}}})", Condition.ToJSON(), IfNode.ToJSON(), ElseNode.ToJSON());
        }
    }
}