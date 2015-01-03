using VLang.AST;
using VLang.Frontends;
using VLang.Runtime;

namespace VLang
{
    public class Engine
    {
        public ExecutionContext Context;
        private Frontend Frontend;
        private InteropManager Interop;

        public Engine()
        {
            Interop = new InteropManager();
            Context = new ExecutionContext();
        }

        public ASTNode Compile(string script)
        {
            Frontend = new DefaultFrontend(script);
            ASTNode node = Frontend.Parse();
            return node;
        }

        public object Execute(string script)
        {
            Frontend = new DefaultFrontend(script);
            ASTNode node = Frontend.Parse();
            return node.GetValue(Context);
        }

        public object Execute(ASTNode node)
        {
            return node.GetValue(Context);
        }
    }
}