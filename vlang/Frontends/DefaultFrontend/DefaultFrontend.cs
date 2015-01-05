using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VLang.AST;
using VLang.AST.Elements;

namespace VLang.Frontends
{
    partial class DefaultFrontend : Frontend
    {
        public Dictionary<string, VLang.AST.Elements.Operator.Operators> StringMap
            = new Dictionary<string, AST.Elements.Operator.Operators>
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

        private Dictionary<int, ASTNode> Groups;
        private List<string> HashTable;

        private Dictionary<string, byte> operator_argc = new Dictionary<string, byte>
        {
            {"+", 2},
            {"-", 2},
            {"*", 2},
            {"/", 2},
            {"%", 2},
            // and here the logic part ends. rly
            {"^", 2},
            {"^^", 2},
            {"&&", 2},
            {"&", 2},
            {"||", 2},
            {"|", 2},
            {"==", 2},
            {"/instance/of/", 2},
            {"/child/of/", 2},
            {"/is/", 2},
            {"!=", 2},
            {">", 2},
            {">=", 2},
            {"<", 2},
            {"<=", 2},
            {"!", 1},
            {".", 2}
        };

        private Dictionary<string, short> operators = new Dictionary<string, short>
        {
            {"(", 0},
            {"+", 1},
            {"-", 1},
            {")", 1},
            {"*", 2},
            {"/", 2},
            {"%", 2},
            // and here the logic part ends. rly
            {"^", 2},
            {"^^", 2},
            {"&&", -2},
            {"&", 2},
            {"||", -2},
            {"|", 2},
            {"==", -1},
            {"/instance/of/", -1},
            {"/child/of/", -1},
            {"/is/", -1},
            {"!=", -1},
            {">", 0},
            {">=", 0},
            {"<", 0},
            {"<=", 0},
            {"!", 3},
            {"=", 4},
            {".", 3}
        };

        private Dictionary<string, string> StringValues;

        public DefaultFrontend(string script)
            : base(script)
        {
            HashTable = new List<string>();
            Groups = new Dictionary<int, ASTNode>();
            StringValues = new Dictionary<string, string>();
        }

        private enum ElementType
        {
            Bool,
            Integer,
            Hex,
            Double,
            Octan,
            Variable,
            Function,
            Assignment,
            Cast,
            Operator,
            AnonymousFunction,
            NamedFunction,
            If,
            IfWithElse,
            Throw,
            Catch,
            Finally,
            Using,
            New,
            Import,
            LoadAssembly
        }

        public override ASTNode Parse()
        {
            HashTable = new List<string>();
            ASTNode root = new ASTNode();

            StripCommentsAndWhiteSpace();
            Stack<int> braces = new Stack<int>();
            int num = 0;
            for (int i = 0; i < Script.Length; i++)
            {
                if (Script[i] == '{')
                {
                    braces.Push(i);
                }
                if (Script[i] == '}')
                {
                    int start = braces.Pop();
                    num++;
                    Groups.Add(num, new DefaultFrontend(Script.Substring(start + 1, i - start - 1)).Parse());
                    Script = Script.Remove(start, i - start + 1);
                    bool embedded = start < Script.Length ? Script[start] == ')' || Script.Substring(start, 5).Trim().StartsWith("else") : false;
                    string name = "_reserved_codegroup_" + num.ToString() + (embedded ? "" : ";");
                    i = start + name.Length - 1;
                    Script = Script.Insert(start, name);
                }
            }
            for (int t = 0; t < Groups.Count; t++)
            {
                var inserter = new KeyValuePair<int, ASTNode>(Groups.ElementAt(t).Key, Groups.Values.ElementAt(t));
                Groups.Remove(Groups.ElementAt(t).Key);
                Groups.Add(inserter.Key, inserter.Value);
                t++;
            }
            foreach (var elem in StringValues)
            {
                root.Add(new Assignment(ToRPN(elem.Key), new Expression(new List<IASTElement>
                {
                    new Value(elem.Value)
                })));
            }
            List<ASTNode> tokens = new List<ASTNode>();
            StringBuilder buffer = new StringBuilder();
            Action flushBuffer = () =>
            {
                string result = buffer.ToString();
                buffer = new StringBuilder();
                root.Add(ToRPN(result));
            };
            for (int i = 0; i < Script.Length; i++)
            {
                char c = Script[i];
                if (c == ';')
                {
                    flushBuffer();
                }
                else
                {
                    buffer.Append(c);
                }
            }
            root.SetGroups(Groups);
            return root;
        }

        public IASTElement ToRPN(string exp)
        {
            exp = exp.Trim();
            // i dont know if it will work. but it should anyway it should get back in there stipped
            // with : by formatexpression
            //if (Flatten(NormalizeBraces(exp)).Contains(":")) return new string[] { exp };
            if (Flatten(NormalizeBraces(exp)).Contains("=") && !Flatten(NormalizeBraces(exp)).Contains("=="))
            {
                return CreateExpression(new string[] { exp });
            }
            if (GetElementType(NormalizeBraces(exp)) == ElementType.AnonymousFunction)
            {
                return CreateExpression(new string[] { exp });
            }
            if (Flatten(NormalizeBraces(exp)).StartsWith("return "))
            {
                return CreateExpression(new string[] { exp });
            }
            if (Flatten(NormalizeBraces(exp)).StartsWith("using "))
            {
                return CreateExpression(new string[] { exp });
            }
            List<string> output = new List<string>();
            Stack<string> rpns = new Stack<string>();

            Dictionary<string, string> stubs = new Dictionary<string, string>(); // THIS WILL HANDLE FUNCIONS TO REPLACE AFTERALL

            Match funccc = Regex.Match(exp, "([A-z0-9.]+)(\\()([^\\)]+)(\\))");

            while (funccc.Success)
            {
                exp = hashFunction(exp, funccc, ref stubs);
                funccc = Regex.Match(exp, "([A-z0-9]+)(\\()([^\\)]+)(\\))");
            }
            funccc = Regex.Match(exp, "([A-z0-9]+)(\\()(\\))");
            while (funccc.Success)
            {
                exp = hashFunction(exp, funccc, ref stubs);
                funccc = Regex.Match(exp, "([A-z0-9]+)(\\()(\\))");
            }

            List<string> sorted = operators.Keys.OrderByDescending(a => a.Length).ToList();
            List<string> randomizer = new List<string>();
            int iter = 0;
            randomizer.AddRange
            (
                sorted.ConvertAll<string>
                (
                    new Converter<string, string>
                    (
                        delegate(string a)
                        {
                            return "\x1\x2" + iter++ + "\x2\x1";
                        }
                    )
                )
            );

            // List<String> bigger = sorted.FindAll(a => a.Length > 1);

            exp = exp.Replace("++", "\x1\x3");
            exp = exp.Replace("--", "\x1\x2");

            iter = 0;
            foreach (string op in sorted)
            {
                exp = exp.Replace(op, randomizer[iter]);
                iter++;
            }
            iter = 0;
            foreach (string op in randomizer)
            {
                exp = exp.Replace(op, "\x5" + sorted[iter] + "\x5");
                iter++;
            }

            exp = exp.Replace("\x1\x3", "++").Replace("\x1\x2", "--");

            List<string> elems = exp.Split('\x5').ToList();

            elems.RemoveAll(a => a.Length == 0);

            string previous = "";
            bool NegateNext = false;
            int level = -1337; // elite
            foreach (string atm in elems)
            {
                if (!operators.Keys.Contains(atm))
                {
                    output.Add(NegateNext ? "-" + atm : atm);
                    NegateNext = false;
                }
                else
                {
                    if (atm == "(") rpns.Push(atm);
                    else if (atm == ")")
                    {
                        while (rpns.Peek() != "(") output.Add(rpns.Pop());
                        rpns.Pop(); // POOPING '('
                    }
                    else if (atm == "-" && operators.ContainsKey(previous)) //minus symbol like 1+-2
                    {
                        NegateNext = true;

                        // and dont do anything
                    }
                    else
                    {
                        if (operators[atm] > level || rpns.Count == 0)
                        {
                            rpns.Push(atm);
                            level = operators[atm];
                        }
                        else
                        {
                            while (rpns.Count > 0 && operators[rpns.Peek()] >= operators[atm]) output.Add(rpns.Pop());
                            rpns.Push(atm);
                        }
                    }
                }
                previous = atm;
            }
            while (rpns.Count > 0)
            {
                output.Add(rpns.Pop());
            }
            for (int i = 0; i < output.Count; i++)
            {
                foreach (var stub in stubs.Reverse())
                {
                    output[i] = output[i].Replace(stub.Key, stub.Value);
                }
            }
            return CreateExpression(output.Where(a => a != ".").ToArray());
        }

        private int CountBraces(string input)
        {
            int br1 = 0, br2 = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '{') br1++;
                if (input[i] == '}') br1--;
                if (input[i] == '(') br2++;
                if (input[i] == ')') br2--;
            }
            return br1 + br2;
        }

        private IASTElement CreateExpression(string[] rpn)
        {
            var list = new List<IASTElement>();
            foreach (string element2 in rpn)
            {
                string element = element2.Trim();
                switch (GetElementType(element))
                {
                    case ElementType.Integer:
                    case ElementType.Octan:
                    case ElementType.Hex:
                        list.Add(ASTCreator.ParseInt(element));
                        break;

                    case ElementType.Double:
                        list.Add(ASTCreator.ParseFloat(element));
                        break;

                    case ElementType.Bool:
                        list.Add(ASTCreator.ParseBool(element));
                        break;

                    case ElementType.Operator:
                        list.Add(new Operator(StringMap[element], operator_argc[element]));
                        break;

                    case ElementType.Assignment:
                        {
                            string[] split = GammaSplitOne(element, '=');
                            string name = split[0];
                            string exp = split[1];
                            list.Add(new Assignment(ToRPN(name.Trim()), ToRPN(exp)));

                        }
                        break;

                    case ElementType.Variable:
                        {
                            if (element.StartsWith("return "))
                            {
                                string expr = element.Substring(6).TrimStart();
                                list.Add(new Return(ToRPN(expr)));
                            }
                            else if (element.StartsWith("using "))
                            {
                                string expr = element.Substring(6).TrimStart();
                                list.Add(new Using(expr));
                            }
                            else if (element.StartsWith("import "))
                            {
                                string expr = element.Substring(6).TrimStart();
                                list.Add(new Return(ToRPN(expr)));
                            }
                            else if (element.StartsWith("mixin "))
                            {
                                string expr = element.Substring(6).TrimStart();
                                list.Add(new Return(ToRPN(expr)));
                            }
                            else
                            {
                                list.Add(new Name(element));
                            }
                        }
                        break;
                    case ElementType.New:
                        {
                            element = element.Substring(3).Trim();
                            if (Flatten(element).Contains('.')) return ToRPN(element);

                            IASTElement name = ToRPN(Flatten(element));
                            string[] arguments = GammaSplit(TrimBraces(CutExpression(element)), ',');
                            list.Add(new New(name, arguments.Select<string, IASTElement>(a => ToRPN(a)).ToList()));
                        }
                        break;

                    case ElementType.Function:
                        {
                            if (Flatten(element).Contains('.')) return ToRPN(element);

                            IASTElement name = ToRPN(Flatten(element));
                            string[] arguments = GammaSplit(TrimBraces(CutExpression(element)), ',');
                            list.Add(new FunctionCall(name, arguments.Length == 1 && arguments[0] == "" ? new Expression[0] : arguments.Select<string, IASTElement>(a => ToRPN(a)).ToArray()));
                        }
                        break;

                    case ElementType.AnonymousFunction:
                        {
                            string[] arguments = TrimBraces(CutExpression(element)).Split(',').Select(a => a.Trim()).ToArray();
                            int groupId = int.Parse(Flatten(element).Trim().Replace("_reserved_codegroup_", ""));
                            list.Add(new Value(new Runtime.Function(arguments.ToList(), groupId)));
                        }
                        break;

                    case ElementType.NamedFunction:
                        {
                            string[] arguments = TrimBraces(CutExpression(element)).Split(',').Select(a => a.Trim()).ToArray();
                            string name = FlattenKeepBraces(element).Split('(')[0];
                            int groupId = int.Parse(Flatten(element).Trim().Replace(name + "_reserved_codegroup_", ""));
                            list.Add(new FunctionDefinition(name, arguments, groupId));
                        }
                        break;

                    case ElementType.If:
                        {
                            IASTElement expr = ToRPN(CutExpression(element).Trim());
                            string sub = Flatten(element);
                            int groupId1 = int.Parse(sub.Trim().Replace("if_reserved_codegroup_", ""));
                            list.Add(new Conditional(expr, groupId1));
                        }
                        break;

                    case ElementType.IfWithElse:
                        {
                            string exp = CutExpression(element);
                            IASTElement expr = ToRPN(exp.Trim());
                            string[] sub = Flatten(element.Replace(" ", "")).Split(new string[] { "else" }, 2, StringSplitOptions.RemoveEmptyEntries);
                            int groupId1 = int.Parse(sub[0].Trim().Replace("if_reserved_codegroup_", ""));
                            int groupId2 = int.Parse(sub[1].Trim().Replace("_reserved_codegroup_", ""));
                            list.Add(new Conditional(expr, groupId1, groupId2));
                        }
                        break;
                }
            }
            return list.Count == 1 ? list[0] : new Expression(list);
        }

        private String CutExpression(String str)
        {
            String output = str;
            Int32 iter = 0, bracelevel = 0, startIndex = 0;
            Boolean ready = false;
            while (iter < str.Length)
            {
                if (str[iter] == '(')
                {
                    bracelevel++;
                    if (!ready) startIndex = iter;
                    ready = true;
                }
                if (str[iter] == ')')
                {
                    bracelevel--;
                    if (bracelevel == 0 && ready)
                    {
                        return str.Substring(startIndex + 1, iter - startIndex - 1);
                    }
                }
                iter++;
            }

            return output;
        }

        private String Flatten(String input, Int64 level = 0)
        {
            String ret = "";
            Int32 iter = 0;
            Int32 bracelevel = 0;
            while (iter < input.Length)
            {
                if (input[iter] == '(') bracelevel++;
                if (bracelevel <= level) ret += input[iter];
                if (input[iter] == ')') bracelevel--;
                iter++;
            }
            return ret;
        }

        private String FlattenKeepBraces(String input, Int64 level = 0)
        {
            String ret = "";
            Int32 iter = 0;
            Int32 bracelevel = 0;
            while (iter < input.Length)
            {
                if (input[iter] == '(') { bracelevel++; ret += input[iter]; }
                if (bracelevel <= level) ret += input[iter];
                if (input[iter] == ')') { bracelevel--; ret += input[iter]; }
                iter++;
            }
            return ret;
        }

        private string[] GammaSplitOne(String str, char sep)
        {
            if (!str.Contains(sep)) return new string[] { str };
            Int32 bracelevel = 0, iter = 0;
            List<Int32> indexes = new List<Int32>();
            indexes.Add(0);
            List<String> ret = new List<String>();
            while (iter < str.Length)
            {
                if (str[iter] == '(') bracelevel++;
                else if (str[iter] == ')') bracelevel--;
                else if (bracelevel == 0 && str[iter] == sep)
                {
                    ret.Add(str.Substring(0, iter));
                    ret.Add(str.Substring(iter + 1));
                    break;
                }
                ++iter;
            }

            return ret.ToArray();
        }

        private ElementType GetElementType(String element)
        {
            if (element == "false" || element == "true") return ElementType.Bool;
            element = FlattenKeepBraces(element);
            if (operators.Keys.Contains(element)) return ElementType.Operator;
            if (Regex.IsMatch(element, @"[^0-7]") == false && element.StartsWith("0")) return ElementType.Octan;
            else if (Regex.IsMatch(element, @"[^0-9]") == false) return ElementType.Integer;
            else if (Regex.IsMatch(element, @"[^0-9.]") == false) return ElementType.Double;
            else if (Regex.IsMatch(element, @"[^0-9a-fA-Fx]") == false && element.StartsWith("0x")) return ElementType.Hex;
            else if (Regex.IsMatch(element, @".+?(`).+?") == true && Regex.IsMatch(element, @"[\(\)=]") == false) return ElementType.Cast;
            else if (Regex.IsMatch(element, @"^([^=]+)(=)([^=]+)$") == true) return ElementType.Assignment;
            else if (Regex.IsMatch(element, @"[\(\)]") == false) return ElementType.Variable;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"^if\(\)_reserved_codegroup_([0-9]+)$") == true) return ElementType.If;
            else if (Regex.IsMatch(element.Replace(" ", ""), @".+\(\)_reserved_codegroup_([0-9]+)$") == true) return ElementType.NamedFunction;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"\)_reserved_codegroup_([0-9]+)$") == true) return ElementType.AnonymousFunction;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"^if\(\)_reserved_codegroup_([0-9]+)else_reserved_codegroup_([0-9]+)$") == true) return ElementType.IfWithElse;
            else if (element.Trim().StartsWith("new ")) return ElementType.New;
            else return ElementType.Function;
        }

        private string hashFunction(string exp, Match match, ref Dictionary<String, String> stubs)
        {
            int bracecount = 0, ite = match.Length;
            int hashingStart = match.Index, hashingLength = match.Length;
            while (--ite > 0)
            {
                // reversed, keep it on mind during next ToRPN fuckage...
                if (exp[ite] == ')') bracecount++;
                if (exp[ite] == '(')
                {
                    bracecount--;
                    if (bracecount == 0)
                    {
                        hashingStart = ite;
                        hashingLength -= ite;
                        break;
                    }
                }
            }
            String hash = RandomHash();
            stubs.Add(hash, exp.Substring(hashingStart, hashingLength));
            exp = exp.Remove(hashingStart, hashingLength);
            exp = exp.Insert(hashingStart, hash);
            return exp;
        }

        private String NormalizeBraces(String str)
        {
            String output = str;
            Int32 iter = 0, bracelevel = 0;
            while (iter < str.Length)
            {
                if (str[iter] == '(') bracelevel++;
                if (str[iter] == ')') bracelevel--;
                iter++;
            }
            if (bracelevel != 0)
            {
                // GOT YA MOTHERFUCKER
                if (bracelevel < 0) for (; bracelevel < 0; bracelevel++) output = output.Insert(0, "(");
                else if (bracelevel > 0) for (; bracelevel > 0; bracelevel--) output += ")";
            }
            /*while (output.EndsWith(")") && output.StartsWith("("))
            {
                output = output.Substring(1, output.Length - 2);
            }*/
            return output;
        }

        private String RandomHash()
        {
            string ret = "";
            var rand = new Random((Int32)(DateTime.Now.Ticks % 256));
            Int64 entrophy = 8;
            while (entrophy-- > 0)
            {
                ret += (char)rand.Next('a', 'z');
            }

            while (HashTable.Contains(ret))
            {
                //Console.WriteLine(ret);
                ret += (char)rand.Next('a', 'z');
            }
            HashTable.Add(ret);

            return ret;
        }

        private void StripCommentsAndWhiteSpace()
        {
            Script = Script.Replace("\r\n", "\n");
            Int64 i = 0;
            int iter = 0, last = 0;
            bool inString = false;
            bool specialString = false;
            while (iter < Script.Length)
            {
                if (Script[iter] == '\'' && !inString)
                {
                    inString = true;
                    last = iter;
                }
                else if ((iter == 0 || Script[iter - 1] != '\\') && Script[iter] == '\'' && inString && !specialString)
                {
                    inString = false;
                    i++;
                    String value = Script.Substring(last + 1, iter - last - 1);
                    value = value.Replace("\\\\", "\xff\xfe\0xfd");
                    value = value.Replace("\\n", "\n");
                    value = value.Replace("\\t", "\t");
                    value = value.Replace("\\r", "\r");
                    value = value.Replace("\\0", "\0");
                    //value = value.Replace("\\x", "\x");
                    value = value.Replace("\\'", "'");
                    value = value.Replace("\xff\xfe\0xfd", "\\");

                    StringValues.Add("__rcs" + i.ToString(), value);

                    Script = Script.Remove(last, iter - last + 1);
                    String insertion = "__rcs" + i.ToString();
                    Script = Script.Insert(last, "__rcs" + i.ToString());
                    iter -= iter - last + insertion.Length - 1;
                }
                iter++;
            }

            Script = Regex.Replace(Script, "(\\/\\/)([^\\n]*)", "", RegexOptions.Singleline);
            Script = Regex.Replace(Script, "(\\/\\*[\\d\\D]*?\\*\\/)", "", RegexOptions.Multiline);

            int brace_diff = CountBraces(Script);
            if (brace_diff != 0)
            {
                throw new Exception("Tokenization failed: Missing braces: " + brace_diff.ToString());
            }

            Script = Regex.Replace(Script, "[\r\n\t]", "");
            while (Script.IndexOf("  ") != -1) Script = Script.Replace("  ", " ");
            Script = Script.Replace(" instance of ", "/instance/of/")
                .Replace(" child of ", "/child/of/")
                .Replace(" is ", "/is/")
                .Replace(" implements ", "/implements/")
                .Replace(" as ", "/as/");
            while (Script.IndexOf(";;") != -1) Script = Script.Replace(";;", ";");
            // now we're clean
        }
        static private string[] GammaSplit(String str, char sep)
        {
            if (!str.Contains('(')) return str.Split(sep);
            Int32 bracelevel = 0, iter = 0;
            List<Int32> indexes = new List<Int32>();
            indexes.Add(0);
            while (iter < str.Length)
            {
                if (str[iter] == '(') bracelevel++;
                else if (str[iter] == ')') bracelevel--;
                else if (bracelevel == 0 && str[iter] == sep) indexes.Add(iter);
                ++iter;
            }
            indexes.Add(str.Length);
            List<String> ret = new List<String>();
            for (iter = 1; iter < indexes.Count; iter++)
            {
                ret.Add(
                    str.Substring(
                        indexes[iter - 1],
                        indexes[iter] - indexes[iter - 1]
                    ).TrimEnd(new char[] { sep }).TrimStart(new char[] { sep })
                );
            }
            return ret.ToArray();
        }
        private String TrimBraces(String str)
        {
            String output = str;
            while (output.EndsWith(")") && output.StartsWith("("))
            {
                output = output.Substring(1, output.Length - 2);
            }
            return output;
        }
    }
}