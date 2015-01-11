using System;
using VLang.AST.Elements;

namespace VLang.AST
{
    internal class ASTCreator
    {
        public static Value ParseBool(string element)
        {
            return new Value(element == "true");
        }

        public static Value ParseFloat(string element)
        {
            double floatout;
            if (double.TryParse(element.Replace('.', ','), System.Globalization.NumberStyles.Float, System.Globalization.DateTimeFormatInfo.InvariantInfo, out floatout))
            {
                return new Value(floatout);
            }
            else throw new Exception("Float cast failed");
        }

        public static Value ParseInt(string element)
        {
            Int64 intout;
            if (Int64.TryParse(element, out intout))
            {
                return new Value(intout);
            }
            else throw new Exception("Integer cast failed");
        }
    }
}