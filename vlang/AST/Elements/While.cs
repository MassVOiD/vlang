using System;

namespace VLang.AST.Elements
{
    public class While : ASTElement, IASTElement
    {
        public IASTElement Condition;
        public IASTElement Node;

        public While(IASTElement condition, IASTElement branch)
        {
            Condition = condition;
            Node = branch;
        }
    }
}