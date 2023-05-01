using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPL_PS2_REPACK
{
    public static class Utils
    {
        public static string ReturnValidHexValue(string cont)
        {
            string res = "";
            foreach (var c in cont)
            {
                if (char.IsDigit(c)
                    || c == 'A'
                    || c == 'B'
                    || c == 'C'
                    || c == 'D'
                    || c == 'E'
                    || c == 'F'
                    )
                {
                    res += c;
                }
            }
            return res;
        }

        public static string ReturnValidDecValue(string cont)
        {
            string res = "";
            foreach (var c in cont)
            {
                if (char.IsDigit(c))
                {
                    res += c;
                }
            }
            return res;
        }

        public static string ReturnValidFloatValue(string cont)
        {
            bool Dot = false;
            bool negative = false;

            string res = "";
            foreach (var c in cont)
            {
                if (negative == false && c == '-')
                {
                    res = c + res;
                    negative = true;
                }

                if (Dot == false && c == '.')
                {
                    res += c;
                    Dot = true;
                }
                if (char.IsDigit(c))
                {
                    res += c;
                }
            }
            return res;
        }
    }

}
