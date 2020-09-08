using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileReader
{
    public static class FirstLabelRules
    {
        public static bool readLineStart = false;
        public static bool readLineEnd = false;
        public static string startToken1 = "--------------------------------------------------------------------------------------------------------------------------------------------------------";
        public static string startToken2 = "check#";
        public static string endToken = "--------------------------------------------------------------------------------------------------------------------------------------------------------";

    }
    public static class SecondLabelRules
    {
        public static  int headerColumnCount=9;
        public static int LineColumnCount = 3;
        public static string startToken = "Line Item";
        public static string midToken = "CONTRACTUAL OBLIGATIONS";
        public static string endToken = "PATIENT RESPONSIBILITY";
        public static bool LineItemStart = false;
        public static bool LineItemEnd = false;
        
    }
}
