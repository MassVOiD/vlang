using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class While : ASTElement, IASTElement
    {
        public IASTElement Condition;
        public IASTElement Node;

        public While(IASTElement condition, IASTElement branch)
        {
            Condition = condition;
            Node = branch;
        }

        public object GetValue(ExecutionContext context)
        {
            object value = null;
            while ((bool)Condition.GetValue(context)) value = Node.GetValue(context);
            return value;
        }

        public override string ToJSON()
        {
            return String.Format("while({0}){{{1}}}", Condition.ToJSON(), Node.ToJSON());
        }
    }
}