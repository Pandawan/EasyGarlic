using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EasyGarlic {
    public class Utilities {
        private static Dictionary<string, int> hashUnits = new Dictionary<string, int>()
        {
            { "H/S", 0 },
            { "KH/S", 3 },
            { "MH/S", 6 },
            { "GH/S", 9 },
            { "TH/S", 12 },
            { "PH/S", 15 },
            { "EH/S", 18 },
            { "ZH/S", 21 },
            { "YH/S", 24 }
        };

        // Converts any kH/s or other to H/s (base)
        public static double ConvertHash(string value, string unit)
        {
            if (String.IsNullOrEmpty(value))
            {
                return 0;
            }

            // 0 = all, 1 = number, 2 = unit
            // Regex (([\d] +[\d\s\.,/] *)\s([A - Za - z\/] +[^\s\d]))
            Match match = Regex.Match(value, @"(([\d]+[\d\s\.,/]*)\s([A-Za-z\/]+[^\s\d]))");
            if (match.Success)
            {
                // GDI Americans use . while others use , why you gotta do this
                double initialValue = double.Parse(match.Groups[2].Value.Replace(',', '.'), CultureInfo.GetCultureInfo("en-US"));
                double multiplier = (hashUnits.ContainsKey(match.Groups[3].Value.ToUpper()) ? hashUnits[match.Groups[3].Value.ToUpper()] : 0) - (hashUnits.ContainsKey(unit.ToUpper()) ? hashUnits[unit.ToUpper()] : 0);

                return (initialValue * Math.Pow(10, multiplier));
            }

            return 0;
        }

        // Converts a hashrate to a string hashrate (from one unit to another)
        public static string ConvertHashString(string value, string unit)
        {
            return ConvertHash(value, unit) + " " + unit.ToUpper();
        }

        // Adds two hashrates together (with unit) and returns a value with the unit from the first one
        public static string AddHashes(string x, string y)
        {
            // TODO: Maybe pick the best unit between x and y rather than just x (convert best unit to # using dictionary and find smaller or bigger one)
            string bestUnit = Regex.Match(x.ToUpper(), @"(([\d]+[\d\s\.,/]*)\s([A-Za-z\/]+[^\s\d]))").Groups[3].Value;

            double convertedX = ConvertHash(x.ToUpper(), bestUnit);

            double convertedY = ConvertHash(y.ToUpper(), bestUnit);

            string sum = (double)(convertedX + convertedY) + " " + bestUnit;

            return sum;
        }

        public static double GetBaseLog(double x, double y)
        {
            return Math.Log(y) / Math.Log(x);
        }

        public static string IDToTitle(string id)
        {
            if (id.Contains("nvidia"))
            {
                return "Nvidia GPU";
            }

            if (id.Contains("amd"))
            {
                return "AMD GPU";
            }

            if (id.Contains("cpu"))
            {
                if (id.Contains("_alt"))
                {
                    return "CPU opt";
                }
                else
                {
                    return "CPU";
                }
            }

            return "";
        }
    }
}
