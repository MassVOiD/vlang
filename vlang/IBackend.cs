using System;
using VLang.AST;

namespace VLang.Frontends
{
    public interface IBackend
    {
        object Execute(ASTNode Root);
    }
}