using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class For : ASTElement, IASTElement
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

        public object GetValue(ExecutionContext context)
        {
            object value = null;
            Before.GetValue(context);
            while ((bool)Condition.GetValue(context))
            {
                value = Node.GetValue(context);
                After.GetValue(context);
            }
            return value;
        }

        public override string ToJSON()
        {
            return String.Format("for({0}, {1}, {2}){{{3}}}", Before.ToJSON(), Condition.ToJSON(), After.ToJSON(), Node.ToJSON());
        }
    }
}