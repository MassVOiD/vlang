using System;
using VLang.AST;

namespace VLang.Frontends
{
    public interface IFrontend
    {
        ASTNode Parse(string script);
    }
}