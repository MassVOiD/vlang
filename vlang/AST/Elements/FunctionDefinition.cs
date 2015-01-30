using System;
using System.Linq;

namespace VLang.AST.Elements
{
    public class FunctionDefinition : ASTElement, IASTElement
    {
        public string[] Arguments;
        public int Body;
        public string Name;
        public string[] Modificators;

        public FunctionDefinition(string name, string[] modificators, string[] args, int body)
        {
            Name = name;
            Arguments = args;
            Modificators = modificators;
            Body = body;
        }
    }
}