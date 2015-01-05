﻿using System;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Mixin : ASTElement, IASTElement
    {
        private IASTElement Expression;

        public Mixin(IASTElement expression)
        {
            Expression = expression;
        }

        public object GetValue(ExecutionContext c)
        {
            return Expression.GetValue(c);
        }

        public override string ToJSON()
        {
            return String.Format("Return({0})", Expression.ToJSON());
        }
    }
}