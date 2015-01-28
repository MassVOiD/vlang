using System.Collections.Generic;
using VLang.AST;
using VLang.Frontends;

namespace VLang
{
    public class Engine
    {
        public static Dictionary<int, ASTNode> Groups;
        private Frontend Frontend;

        public Engine()
        {
        }

        public ASTNode Compile(string script)
        {
            Frontend = new DefaultFrontend(script);
            ASTNode node = Frontend.Parse();
            return node;
        }

    }
}