using System;

namespace VLang.AST.Elements
{
    public class For : ASTElement, IASTElement
    {
        public IASTElement Condition, Before, After;
        public IASTElement Node;

        public For(IASTElement condition, IASTElement before, IASTElement after, IASTElement branch)
        {
            Condition = condition;
            Before = before;
            After = after;
            Node = branch;
        }

        public override string ToJSON()
        {
            return String.Format("for({0}, {1}, {2}){{{3}}}", Before.ToJSON(), Condition.ToJSON(), After.ToJSON(), Node.ToJSON());
        }
    }
}