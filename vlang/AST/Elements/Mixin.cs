using System;
namespace VLang.AST.Elements
{
    public class Mixin : ASTElement, IASTElement
    {
        public IASTElement Expression;

        public Mixin(IASTElement expression)
        {
            Expression = expression;
        }

        public override string ToJSON()
        {
            return String.Format("mixin {0}", Expression.ToJSON());
        }
    }
}