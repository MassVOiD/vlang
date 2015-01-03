using System;
using VLang.AST;

namespace VLang.Frontends
{
    internal abstract class Frontend
    {
        public Frontend(string script)
        {
            Script = script;
        }

        public String Script { get; set; }

        public abstract ASTNode Parse();
    }
}