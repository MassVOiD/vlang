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
    }
}