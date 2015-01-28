using System;

namespace VLang.AST.Elements
{
    public class Return : ASTElement, IASTElement
    {
        public IASTElement Expression;

        public Return(IASTElement expression)
        {
            Expression = expression;
        }

        public override string ToJSON()
        {
            return String.Format("return {0}", Expression.ToJSON());
        }
    }
}