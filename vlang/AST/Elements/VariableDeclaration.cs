﻿using System;
using System.Collections.Generic;

namespace VLang.AST.Elements
{
    public class VariableDeclaration : ASTElement, IASTElement
    {
        public string TypeName;
        public string Target;
        public IASTElement Value;

        public VariableDeclaration(string type, string target, IASTElement value)
        {
            TypeName = type;
            Target = target;
            Value = value;
        }
    }
}