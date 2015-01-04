using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Mixin : ASTElement, IASTElement
    {
        private Expression Expression;

        public Mixin(Expression expression)
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