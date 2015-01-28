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
    }
}