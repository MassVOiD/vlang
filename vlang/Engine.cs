using System.Collections.Generic;
using VLang.AST;
using VLang.Frontends;

namespace VLang
{
    public class Engine
    {
        private IFrontend Frontend;
        private IBackend Backend;

        public Engine(IFrontend frontend, IBackend backend)
        {
            Frontend = frontend;
            Backend = backend;
        }

        public ASTNode Compile(string script)
        {
            Frontend = new DefaultFrontend();
            ASTNode node = Frontend.Parse(script);
            return node;
        }

        public object Execute(ASTNode node){
            return Backend.Execute(node);
        }

        public object Execute(string script)
        {
            return Execute(Compile(script));
        }

    }
}