using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VLang;
using VLang.AST;
using VLang.AST.Elements;
using System.Threading.Tasks;
using System.IO;

namespace JavascriptBackend
{
    public class JavascriptBackend : VLang.Frontends.IBackend
    {
        static Dictionary<string, Operator.Operators> StringMap
               = new Dictionary<string, Operator.Operators>
        {
            {"+", VLang.AST.Elements.Operator.Operators.Add},
            {"-",  VLang.AST.Elements.Operator.Operators.Subtract},
            {"*", VLang.AST.Elements.Operator.Operators.Multiply},
            {"/", VLang.AST.Elements.Operator.Operators.Divide},
            {"%", VLang.AST.Elements.Operator.Operators.Modulo},
            {"^", VLang.AST.Elements.Operator.Operators.Power},
            {"^^", VLang.AST.Elements.Operator.Operators.Xor},
            {"&&", VLang.AST.Elements.Operator.Operators.And},
            {"&", VLang.AST.Elements.Operator.Operators.BAnd},
            {"||", VLang.AST.Elements.Operator.Operators.Or},
            {"|", VLang.AST.Elements.Operator.Operators.BOr},
            {"==", VLang.AST.Elements.Operator.Operators.Equals},
            {"/instance/of/", VLang.AST.Elements.Operator.Operators.InstanceOf},
            {"/child/of/", VLang.AST.Elements.Operator.Operators.ChildOf},
            {"/is/", VLang.AST.Elements.Operator.Operators.Is},
            {"!=", VLang.AST.Elements.Operator.Operators.NotEquals},
            {">", VLang.AST.Elements.Operator.Operators.More},
            {">=", VLang.AST.Elements.Operator.Operators.MoreOrEqual},
            {"<", VLang.AST.Elements.Operator.Operators.Less},
            {"<=", VLang.AST.Elements.Operator.Operators.LessOrEqual},
            {"!", VLang.AST.Elements.Operator.Operators.Not},
            {"=", VLang.AST.Elements.Operator.Operators.Assign},
            {".", VLang.AST.Elements.Operator.Operators.Accessor}
        };
        string revertInfixNotation(Expression exp, ASTNode root)
        {
            var stack = new Stack<IASTElement>();
            var sb = new System.Text.StringBuilder();
            foreach(var element in exp.List)
            {
                if(element is Operator)
                {
                    int argc = ((Operator)element).GetArgumentsCount();
                    string symbol = StringMap.First(a => a.Value == ((Operator)element).Type).Key;
                    if(argc == 2 && stack.Count >= 2)
                    {
                        var v1 = stack.Count != 0 ? stack.Pop() : null;
                        var v2 = stack.Count != 0 ? stack.Pop() : null;
                        if(v1 is Value && v2 is Value)
                        {
                            stack.Push(new Value(((Operator)element).Execute(new dynamic[] { ((Value)v2).Val, ((Value)v1).Val })));
                        }
                        else
                        {
                            sb.Append("(" + Execute(v2, root));
                            sb.Append(symbol);
                            sb.Append(Execute(v1, root) + ")");
                        }
                    }
                    else if(argc == 1 && stack.Count >= 1)
                    {
                        var v1 = stack.Count != 0 ? stack.Pop() : null;
                        sb.Append(symbol);
                        sb.Append(Execute(v1, root));
                    }
                    //while(!(stack.Peek() is Operator)) sb.Append(stack.Pop().ToJSON());
                }
                else
                    stack.Push(element);
            }
            while(stack.Count != 0)
                sb.Append(Execute(stack.Pop(), root));
            return sb.ToString();
        }

        public object Execute(ASTNode node)
        {
            StringBuilder builder = new StringBuilder();
            foreach(var leaf in node)
            {
                builder.Append(Execute(leaf, node) + ';');
            }
            return new JSBeautifyLib.JSBeautify(builder.ToString(), new JSBeautifyLib.JSBeautifyOptions()
            {
                preserve_newlines = true,
                indent_char = ' ',
                indent_size = 4
            }).GetResult();
        }

        public string Execute(IASTElement leaf, ASTNode root)
        {
            StringBuilder builder = new StringBuilder();

            if(leaf is ASTNode)
            {
                builder.Append(String.Format("{0}", String.Join(";",
                       ((ASTNode)leaf).Select<IASTElement, string>(a => Execute(a, root)))));
            }
            if(leaf is Expression)
            {
                builder.Append(String.Format("{0}", revertInfixNotation(leaf as Expression, root)));
            }

            if(leaf is Assignment)
            {
                builder.Append(String.Format("{0} = {1}", Execute(((Assignment)leaf).Target, root), 
                    Execute(((Assignment)leaf).Value, root)));
            }

            if(leaf is VariableDeclaration)
            {
                builder.Append(
                    String.Format(
                        "var {0} = {1}", 
                        ((VariableDeclaration)leaf).Target, 
                        Execute(((VariableDeclaration)leaf).Value, root)));
            }

            if(leaf is Conditional)
            {
                if(((Conditional)leaf).ElseNode != null)
                {
                    builder.Append(String.Format("if({0}){{{1}}}else{{{2}}}", Execute(((Conditional)leaf).Condition, root), 
                        Execute(((Conditional)leaf).IfNode, root), Execute(((Conditional)leaf).ElseNode, root)));
                }
                else
                {
                    builder.Append(String.Format("if({0}){{{1}}}", Execute(((Conditional)leaf).Condition, root), Execute(((Conditional)leaf).IfNode, root)));
                }
            }
            
            if(leaf is Value)
            {
                object Val = ((Value)leaf).Val;
                if(Val is string)
                {
                    builder.Append(String.Format("'{0}'", Val.ToString()));
                }else
                if(Val is float)
                {
                    builder.Append(String.Format("{0}", ((float)Val).ToString().Replace(',', '.')));
                }else 
                if(Val is double)
                {
                    builder.Append(String.Format("{0}", ((double)Val).ToString().Replace(',', '.')));
                }
                else
                {
                    builder.Append(String.Format("{0}", Val.ToString()));
                }
            }
            if(leaf is Name)
            {
                builder.Append(String.Format("{0}", ((Name)leaf).Identifier));
            }
            if(leaf is New)
            {
                builder.Append(String.Format("new {0}({1})", Execute(((New)leaf).Name, root), String.Join(",",
                    ((New)leaf).Arguments.Select<IASTElement, string>(a => Execute(a, root)))));
            }
            if(leaf is For)
            {
                builder.Append(String.Format("for({0}, {1}, {2}){{{3}}}", Execute(((For)leaf).Before, root), Execute(((For)leaf).Condition, root),
                    Execute(((For)leaf).After, root), Execute(((For)leaf).Node, root)));
            }

            if(leaf is Loop)
            {
                builder.Append( String.Format("while(true){{{1}}}", Execute(root.Groups[((Loop)leaf).Node], root)));
            }
            if(leaf is Mixin)
            {

            }
            if(leaf is Return)
            {
                builder.Append(String.Format("return {0}", Execute(((Return)leaf).Expression, root)));
            }



            if(leaf is FunctionCall)
            {
                string Name = Execute(((FunctionCall)leaf).Func, root);
                builder.Append(String.Format("{0}({1})", Name, 
                    String.Join(",", ((FunctionCall)leaf).Arguments.Select<IASTElement, string>(a => Execute(a, root)))));
            }
            if(leaf is FunctionDefinition)
            {
                builder.Append(String.Format("{0}({1}){{{2}}}", ((FunctionDefinition)leaf).Name,
                    String.Join(",", ((FunctionDefinition)leaf).Arguments), Execute(root.Groups[((FunctionDefinition)leaf).Body], root)));
            }
            return builder.ToString();
        }
    }
}
