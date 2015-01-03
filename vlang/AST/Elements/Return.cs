using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Return : ASTElement, IASTElement
    {
        private Expression Expression;

        public Return(Expression expression)
        {
            Expression = expression;
        }

        public object GetValue(ExecutionContext c)
        {
            return Expression.GetValue(c);
        }

        public override string ToJSON()
        {
            return String.Format("Return({0})", Expression.ToJSON());
        }
    }
}