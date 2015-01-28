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

    }
}