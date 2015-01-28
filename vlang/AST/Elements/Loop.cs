using System;

namespace VLang.AST.Elements
{
    public class Loop : ASTElement, IASTElement
    {
        public int Node;

        public Loop(int branch)
        {
            Node = branch;
        }
    }
}