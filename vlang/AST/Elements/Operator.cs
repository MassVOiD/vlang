using System;
using System.Linq;
using VLang.Runtime;

namespace VLang.AST.Elements
{
    internal class Operator : ASTElement, IASTElement
    {
        public int ArgumentsCount;

        public Operators Type;

        public Operator(Operators op, int argumentsCount)
        {
            Type = op;
            ArgumentsCount = argumentsCount;
        }

        public enum Operators
        {
            Add, Subtract, Multiply, Divide, Modulo, Power, And, BAnd, Or, BOr, Equals, InstanceOf,
            ChildOf, Is, NotEquals, More, MoreOrEqual, Less, LessOrEqual, Not, Xor, Assign, Accessor
        }

        public dynamic Execute(dynamic[] arguments)
        {
            dynamic value = null;
            switch (Type)
            {
                case Operators.Add: return arguments[0] + arguments[1];
                case Operators.And: return arguments[0] && arguments[1];
                //case Operators.Assign: return arguments[0] = arguments[1];
                case Operators.BAnd: return arguments[0] & arguments[1];
                case Operators.BOr: return arguments[0] | arguments[1];
                //case Operators.ChildOf: return arguments[0]  arguments[1];
                case Operators.Divide: return arguments[0] / arguments[1];
                case Operators.Equals: return arguments[0] == arguments[1];
                //case Operators.InstanceOf: return arguments[0] + arguments[1];
                //case Operators.Is: return arguments[0] + arguments[1];
                case Operators.Less: return arguments[0] < arguments[1];
                case Operators.LessOrEqual: return arguments[0] <= arguments[1];
                case Operators.Modulo: return arguments[0] % arguments[1];
                case Operators.More: return arguments[0] > arguments[1];
                case Operators.MoreOrEqual: return arguments[0] >= arguments[1];
                case Operators.Multiply: return arguments[0] * arguments[1];
                case Operators.Not: return !arguments[0];
                case Operators.NotEquals: return arguments[0] != arguments[1];
                case Operators.Or: return arguments[0] || arguments[1];
                case Operators.Power: return Math.Pow(arguments[0], arguments[1]);
                case Operators.Subtract: return arguments[0] - arguments[1];
                case Operators.Xor: return arguments[0] ^ arguments[1];
            }
            return value;
        }

        public int GetArgumentsCount()
        {
            return ArgumentsCount;
        }

        public object GetValue(ExecutionContext c)
        {
            return null;
        }

        public override bool HasValue(ExecutionContext context)
        {
            return false;
        }

        public override string ToJSON()
        {
            return String.Format("{0}", Frontends.DefaultFrontend.StringMap.First(a => a.Value == Type).Key);
        }
    }
}