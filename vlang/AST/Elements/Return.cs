using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Return : ASTElement, IASTElement
    {
        public IASTElement Expression;

        public Return(IASTElement expression)
        {
            Expression = expression;
        }

        public object GetValue(ExecutionContext c)
        {
            return Expression.GetValue(c);
        }

        public override string ToJSON()
        {
            return String.Format("return {0}", Expression.ToJSON());
        }
    }
}