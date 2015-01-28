using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VLang.AST;
using VLang.AST.Elements;

namespace VLang.Frontends
{
    public class DefaultFrontend : IFrontend
    {
        public static Dictionary<string, VLang.AST.Elements.Operator.Operators> StringMap
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

        public DefaultFrontend()
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
            LoadAssembly,
            While,
            Loop,
            For,
            Foreach,
            Map,
            Filter
        }

        private ASTNode ParsePrivate(string script)
        {
            ASTNode root = new ASTNode();

            Stack<int> braces = new Stack<int>();
            int num = 0;
            for(int i = 0; i < script.Length; i++)
            {
                if(script[i] == '{')
                {
                    braces.Push(i);
                }
                if(script[i] == '}')
                {
                    int start = braces.Pop();
                    num++;
                    Groups.Add(num, ParsePrivate(script.Substring(start + 1, i - start - 1)));
                    script = script.Remove(start, i - start + 1);
                    bool embedded = start + 5 < script.Length ? script[start] == ')' || script.Substring(start, 5).Trim().StartsWith("else") : false;
                    string name = "_reserved_codegroup_" + num.ToString() + (embedded ? "" : ";");
                    i = start + name.Length - 1;
                    script = script.Insert(start, name);
                }
            }
            for(int t = 0; t < Groups.Count; t++)
            {
                var inserter = new KeyValuePair<int, ASTNode>(Groups.ElementAt(t).Key, Groups.Values.ElementAt(t));
                Groups.Remove(Groups.ElementAt(t).Key);
                Groups.Add(inserter.Key, inserter.Value);
                t++;
            }
            script = NormalizeBraces(script); // crazy really
            string[] expressions = GammaSplit(script, ';');
            foreach(var exp in expressions)
                root.Add(ToRPN(exp));
            root.SetGroups(Groups);
            return root;
        }


        public ASTNode Parse(string script)
        {
            HashTable = new List<string>();
            script = StripCommentsAndWhiteSpace(script);

            ASTNode root = ParsePrivate(script);

            foreach(var elem in StringValues)
            {
                root.Insert(0, new VariableDeclaration("string", elem.Key, new Expression(new List<IASTElement>
                {
                    new Value(elem.Value)
                })));
            }
            return root;
        }

        public IASTElement ToRPN(string exp)
        {
            string[] expressions = GammaSplit(exp.Trim(), ';');
            if (expressions.Length != 1)
            {
                return CreateExpression(expressions);
            }
            else exp = expressions[0];
            if (Regex.IsMatch(exp, @"^_reserved_codegroup_([0-9]+)"))
            {
                return Groups[int.Parse(exp.Replace("_reserved_codegroup_", ""))];
            }
            exp = exp.Trim();
            // i dont know if it will work. but it should anyway it should get back in there stipped
            // with : by formatexpression
            //if (Flatten(NormalizeBraces(exp)).Contains(":")) return new string[] { exp };
            if (GetElementType(NormalizeBraces(exp)) == ElementType.AnonymousFunction)
            {
                return CreateExpression(new string[] { exp });
            }
            if (GetElementType(NormalizeBraces(exp)) == ElementType.While)
            {
                return CreateExpression(new string[] { exp });
            }
            if (GetElementType(NormalizeBraces(exp)) == ElementType.For)
            {
                return CreateExpression(new string[] { exp });
            }
            if (GetElementType(NormalizeBraces(exp)) == ElementType.Foreach)
            {
                return CreateExpression(new string[] { exp });
            }
            if (GetElementType(NormalizeBraces(exp)) == ElementType.Map)
            {
                return CreateExpression(new string[] { exp });
            }
            if (GetElementType(NormalizeBraces(exp)) == ElementType.Filter)
            {
                return CreateExpression(new string[] { exp });
            }
            if (Flatten(NormalizeBraces(exp)).StartsWith("return"))
            {
                return CreateExpression(new string[] { exp });
            }
            if (Flatten(NormalizeBraces(exp)).StartsWith("using"))
            {
                return CreateExpression(new string[] { exp });
            }
            if(Flatten(NormalizeBraces(exp)).Contains("=") && !Flatten(NormalizeBraces(exp)).Contains("==") && !Flatten(NormalizeBraces(exp)).Contains("!="))
            {
                return CreateExpression(new string[] { exp });
            }
            if (Flatten(NormalizeBraces(exp)).StartsWith("new"))
            {
                return CreateExpression(new string[] { exp });
            }
            List<string> output = new List<string>();
            Stack<string> rpns = new Stack<string>();

            Dictionary<string, string> stubs = new Dictionary<string, string>(); // THIS WILL HANDLE FUNCIONS TO REPLACE AFTERALL

            Match funccc = Regex.Match(exp, "([A-z0-9.]+)(\\()([^\\)]+)(\\))");

            while (funccc.Success)
            {
                exp = HashFunction(exp, funccc, ref stubs);
                funccc = Regex.Match(exp, "([A-z0-9]+)(\\()([^\\)]+)(\\))");
            }
            funccc = Regex.Match(exp, "([A-z0-9]+)(\\()(\\))");
            while (funccc.Success)
            {
                exp = HashFunction(exp, funccc, ref stubs);
                funccc = Regex.Match(exp, "([A-z0-9]+)(\\()(\\))");
            } 
            funccc = Regex.Match(exp, "[^A-z_0-9]{1,}[0-9]+(\\.)[0-9]+[^A-z_0-9]*");
            while (funccc.Success)
            {
                exp = exp.Remove(funccc.Groups[1].Index, 1).Insert(funccc.Groups[1].Index, "\x7");
                funccc = Regex.Match(exp, "[^A-z_0-9]{1,}[0-9]+\\.[0-9]+[^A-z_0-9]*");
            }

            List<string> sorted = operators.Keys.OrderByDescending(a => a.Length).Where(a => a != ".").ToList();
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

            elems.RemoveAll(a => a.Length == 0 || a == " ");

            string previous = null;
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
                    else if (atm == "-" && (previous == null || operators.ContainsKey(previous))) //minus symbol like 1+-2
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
                output[i] = output[i].Replace('\x7', '.');
            }
            return CreateExpression(output.ToArray());
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


        private int CountBracesNormal(string input)
        {
            int br2 = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '(') br2++;
                if (input[i] == ')') br2--;
            }
            return br2;
        }

        private int CountBracesCurly(string input)
        {
            int br1 = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '{') br1++;
                if (input[i] == '}') br1--;
            }
            return br1;
        }

        private IASTElement CreateExpression(string[] rpn)
        {
            var list = new List<IASTElement>();
            foreach (string chane in rpn)
            {
                if(chane.Trim().Length == 0)
                    continue;
                string withSpaces = chane.Trim();
                string elementNoSpaces = chane.Replace(" ", "");
                switch(GetElementType(elementNoSpaces))
                {
                    case ElementType.Integer:
                    case ElementType.Octan:
                    case ElementType.Hex:
                    list.Add(ASTCreator.ParseInt(elementNoSpaces));
                        break;

                    case ElementType.Double:
                        list.Add(ASTCreator.ParseFloat(elementNoSpaces));
                        break;

                    case ElementType.Bool:
                        list.Add(ASTCreator.ParseBool(elementNoSpaces));
                        break;

                    case ElementType.Operator:
                        list.Add(new Operator(StringMap[elementNoSpaces], operator_argc[elementNoSpaces]));
                        break;

                    case ElementType.Assignment:
                        {
                            string[] split = GammaSplitOne(withSpaces, '=');
                            string name = split[0];
                            string exp = split[1];
                            if(name.Contains(" ")){
                                string[] split2 = GammaSplitOne(name, ' ');
                                list.Add(new VariableDeclaration(split2[0].Trim(), split2[1].Trim(), ToRPN(exp)));
                            } else {
                                list.Add(new Assignment(ToRPN(name.Trim()), ToRPN(exp)));
                            }
                        }
                        break;

                    case ElementType.Variable:
                        {
                            if(elementNoSpaces.StartsWith("return"))
                            {
                                string expr = elementNoSpaces.Substring(6).TrimStart();
                                list.Add(new Return(ToRPN(expr)));
                            }
                            else
                            {
                                list.Add(new Name(elementNoSpaces));
                            }
                        }
                        break;

                    case ElementType.New:
                        {
                            elementNoSpaces = elementNoSpaces.Substring(3).Trim();
                            IASTElement name = null; ;
                            if(Flatten(elementNoSpaces).Contains(")."))
                                name = ToRPN(Flatten(elementNoSpaces));
                            else if(Flatten(elementNoSpaces).Contains('.'))
                                name = new Value(Flatten(elementNoSpaces));
                            else
                                name = ToRPN(Flatten(elementNoSpaces));
                            string[] arguments = GammaSplit(TrimBraces(CutExpression(elementNoSpaces)), ',');
                            list.Add(new New(name, arguments.Select<string, IASTElement>(a => ToRPN(a)).ToList()));
                        }
                        break;

                    case ElementType.Function:
                        {
                          //  if(Flatten(elementNoSpaces).Contains('.'))
                          //      return ToRPN(elementNoSpaces);

                            IASTElement name = ToRPN(Flatten(elementNoSpaces));
                            string[] arguments = GammaSplit(TrimBraces(CutExpression(elementNoSpaces)), ',');
                            list.Add(new FunctionCall(name, arguments.Length == 1 && arguments[0] == "" ? new Expression[0] : arguments.Select<string, IASTElement>(a => ToRPN(a)).ToArray()));
                        }
                        break;

                    case ElementType.AnonymousFunction:
                        {
                            string[] arguments = TrimBraces(CutExpression(elementNoSpaces)).Split(',').Select(a => a.Trim()).ToArray();
                            int groupId = int.Parse(Flatten(elementNoSpaces).Trim().Replace("_reserved_codegroup_", ""));
                            //list.Add(new Value(new Runtime.Function(arguments.ToList(), groupId)));
                        }
                        break;

                    case ElementType.NamedFunction:
                        {
                            string[] arguments = TrimBraces(CutExpression(elementNoSpaces)).Split(',').Select(a => a.Trim()).ToArray();
                            string name = FlattenKeepBraces(elementNoSpaces).Split('(')[0];
                            int groupId = int.Parse(Flatten(elementNoSpaces).Trim().Replace(name + "_reserved_codegroup_", ""));
                            list.Add(new FunctionDefinition(name, arguments, groupId));
                        }
                        break;

                    case ElementType.If:
                        {
                            IASTElement expr = ToRPN(CutExpression(elementNoSpaces).Trim());
                            string sub = Flatten(elementNoSpaces);
                            int groupId1 = int.Parse(sub.Trim().Replace("if_reserved_codegroup_", ""));
                            list.Add(new Conditional(expr, Groups[groupId1]));
                        }
                        break;

                    case ElementType.While:
                        {
                            string ex = CutExpression(elementNoSpaces);
                            IASTElement expr = ToRPN(ex.Trim());
                            string group = elementNoSpaces.Replace("while(" + ex + ")", "").Trim();
                            list.Add(new While(expr, ToRPN(group)));
                        }
                        break;
                    case ElementType.For:
                        {
                            string expFull = CutExpression(elementNoSpaces);
                            string[] ex = GammaSplit(expFull, ';');
                            IASTElement before = ToRPN(ex[0].Trim());
                            IASTElement expr = ToRPN(ex[1].Trim());
                            IASTElement after = ToRPN(ex[2].Trim());
                            string group = elementNoSpaces.Replace("for(" + expFull + ")", "").Trim();
                            list.Add(new For(expr, before, after, ToRPN(group)));
                        }
                        break;

                    case ElementType.Loop:
                        {
                            string sub = Flatten(elementNoSpaces);
                            int groupId1 = int.Parse(sub.Trim().Replace("loop_reserved_codegroup_", ""));
                            list.Add(new Loop(groupId1));
                        }
                        break;

                    case ElementType.IfWithElse:
                        {
                            string exp = CutExpression(elementNoSpaces);
                            IASTElement expr = ToRPN(exp.Trim());
                            string[] sub = Flatten(elementNoSpaces.Replace(" ", "")).Split(new string[] { "else" }, 2, StringSplitOptions.RemoveEmptyEntries);
                            int groupId1 = int.Parse(sub[0].Trim().Replace("if_reserved_codegroup_", ""));
                            int groupId2 = int.Parse(sub[1].Trim().Replace("_reserved_codegroup_", ""));
                            list.Add(new Conditional(expr, Groups[groupId1], Groups[groupId2]));
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
            if (element.StartsWith("return")) return ElementType.Variable;
            else if (element.Trim().StartsWith("new")) return ElementType.New;
            if (operators.Keys.Contains(element)) return ElementType.Operator;
            if (Regex.IsMatch(element, @"[^0-7]") == false && element.StartsWith("0")) return ElementType.Octan;
            else if (Regex.IsMatch(element, @"[^0-9]") == false) return ElementType.Integer;
            else if (Regex.IsMatch(element, @"[^0-9.]") == false) return ElementType.Double;
            else if (Regex.IsMatch(element, @"[^0-9a-fA-Fx]") == false && element.StartsWith("0x")) return ElementType.Hex;
            else if (Regex.IsMatch(element, @".+?(`).+?") == true && Regex.IsMatch(element, @"[\(\)=]") == false) return ElementType.Cast;
            else if (Regex.IsMatch(element, @"[\(\)=]") == false) return ElementType.Variable;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"^if\(\).+else.+$") == true) return ElementType.IfWithElse;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"^if\(\).+$") == true) return ElementType.If;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"^while\(\).+$") == true) return ElementType.While;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"^for\(\).+$") == true) return ElementType.For;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"^foreach\(\).+$") == true) return ElementType.Foreach;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"^map\(\).+$") == true) return ElementType.Map;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"^filter\(\).+$") == true) return ElementType.Filter;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"^loop\(\).+$") == true) return ElementType.Loop;
            else if (Regex.IsMatch(element.Replace(" ", ""), @".+\(\)_reserved_codegroup_([0-9]+)$") == true) return ElementType.NamedFunction;
            else if (Regex.IsMatch(element.Replace(" ", ""), @"\)_reserved_codegroup_([0-9]+)$") == true) return ElementType.AnonymousFunction;
            else if(Regex.IsMatch(element, @"^([^=]+)(=)([^=]+)$") == true && !element.Contains("!="))
                return ElementType.Assignment;
            else return ElementType.Function;
        }

        string HashFunction(string exp, Match match, ref Dictionary<String, String> stubs)
        {
            if (CountBraces(exp) != 0) throw new Exception("Syntax error");
            int bracecount = 0, ite = exp.Length - 1;
            int hashingStart = match.Index, hashingLength = match.Length;
            while (ite >= 0)
            {
                // reversed, keep it on mind during next ToRPN fuckage...
                if (exp[ite] == ')')
                {
                    if (bracecount == 0) hashingLength = ite;
                    bracecount++;
                }
                if (exp[ite] == '(')
                {
                    bracecount--;
                    if (bracecount == 0)
                    {
                        hashingStart = ite;
                        hashingLength = hashingLength - ite + 1;
                        //break;
                        String hash = RandomHash();
                        stubs.Add(hash, exp.Substring(hashingStart, hashingLength));
                        exp = exp.Remove(hashingStart, hashingLength);
                        exp = exp.Insert(hashingStart, hash);
                        ite = ite - hashingLength + hash.Length;
                    }
                }
                ite--;
            }

            if (CountBraces(exp) != 0) throw new Exception("Syntax parsing error");
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

        private string StripCommentsAndWhiteSpace(string script)
        {
            script = script.Replace("\r\n", "\n");
            Int64 i = 0;
            int iter = 0, last = 0;
            bool inString = false;
            bool specialString = false;
            while (iter < script.Length)
            {
                if (script[iter] == '\'' && !inString)
                {
                    inString = true;
                    last = iter;
                }
                else if ((iter == 0 || script[iter - 1] != '\\') && script[iter] == '\'' && inString && !specialString)
                {
                    inString = false;
                    i++;
                    String value = script.Substring(last + 1, iter - last - 1);
                    value = value.Replace("\\\\", "\xff\xfe\0xfd");
                    value = value.Replace("\\n", "\n");
                    value = value.Replace("\\t", "\t");
                    value = value.Replace("\\r", "\r");
                    value = value.Replace("\\0", "\0");
                    //value = value.Replace("\\x", "\x");
                    value = value.Replace("\\'", "'");
                    value = value.Replace("\xff\xfe\0xfd", "\\");

                    StringValues.Add("__rcs" + i.ToString(), value);

                    script = script.Remove(last, iter - last + 1);
                    String insertion = "__rcs" + i.ToString();
                    script = script.Insert(last, "__rcs" + i.ToString());
                    iter -= iter - last + insertion.Length - 1;
                }
                iter++;
            }

            script = Regex.Replace(script, "(\\/\\/)([^\\n]*)", "", RegexOptions.Singleline);
            script = Regex.Replace(script, "(\\/\\*[\\d\\D]*?\\*\\/)", "", RegexOptions.Multiline);

            int brace_diff = CountBracesCurly(script);
            if (brace_diff != 0)
            {
                throw new Exception("Tokenization failed: Missing {} braces: " + brace_diff.ToString());
            }
            brace_diff = CountBracesNormal(script);
            if (brace_diff != 0)
            {
                throw new Exception("Tokenization failed: Missing () braces: " + brace_diff.ToString());
            }

            script = Regex.Replace(script, "[\r\n\t]", "");
            while (script.IndexOf("  ") != -1) script = script.Replace("  ", " ");
            script = script.Replace(" instance of ", "/instance/of/")
                .Replace(" child of ", "/child/of/")
                .Replace(" is ", "/is/")
                .Replace(" implements ", "/implements/")
                .Replace(" as ", "/as/");
            //script = script.Replace(") ", "");
            //script = script.Replace(" (", "");
            while (script.IndexOf(";;") != -1) script = script.Replace(";;", ";");
            // now we're clean
            return script;
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