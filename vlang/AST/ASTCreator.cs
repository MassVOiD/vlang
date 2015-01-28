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
            float floatout;
            if(float.TryParse(element.Replace('.', ','), System.Globalization.NumberStyles.Float, System.Globalization.DateTimeFormatInfo.InvariantInfo, out floatout))
            {
                return new Value(floatout);
            }
            else throw new Exception("Float cast failed");
        }

        public static Value ParseInt(string element)
        {
            int intout;
            if(int.TryParse(element, out intout))
            {
                return new Value(intout);
            }
            else throw new Exception("Integer cast failed");
        }
    }
}