﻿using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.Broadcast.ApplicationServices.Inventory.ProgramMapping
{

    public interface IProgramMappingCleanupEngine
    {
        string GetCleanProgram(string programName);
    }
    public class ProgramMappingCleanupEngine : IProgramMappingCleanupEngine
    {
        public string GetCleanProgram(string programName)
        {
            var result = programName;

            result = _ApplyRemoval(result);
            result = _ApplyReplacement(result);
            result = result.Trim();

            return result;
        }

        private string _ApplyRemoval(string name)
        {
            var filterPatterns = new List<string>()
            {
                @"^\**",
                @"^\#",
                @"\(2X\)", @"\(X2\)", @"2X", @"X2",
                @"\(TENTATIVE\)",
                @"\(R\)",
                @"\[E\]",
                @"\(O\)",
                @"\(EM\)",
                @"\sAT\s\d\d?:?\d?\d?[AP]?M?-\d\d?:?\d?\d?[AP]?M?",
                @"\@\s?\d\d?:?\d?\d?[AP]?M?-\d\d?:?\d?\d?[AP]?M?",
                @"\sAT\s\d\d?:?\d?\d?[AP]?M?",
                @"\@\s?\d\d?:?\d?\d?[AP]?M?",
                @"\d\d?:?\d?\d?[AP]?M?-\d\d?:?\d?\d?[AP]?M?",
                @"\(\d\d?:?\d?\d?[AP]?M?\)",
                @"\s\d\d?:?\d?\d?[AP]M?",
                @"^\d\d?:?\d?\d?[AP]M?\s",
                @"[\s-]SU[N]?$", @"[\s-]SA[T]?$",
                 @"^SU[N]?[\s-]", @"^SA[T]?[\s-]",
                @"\sRPT",
                //@"\sWKND$",
                @"\sENCORE$",
                @"^24HOUR", @"^24 HOUR", @"24-HOUR",
                @"FOLLOWING NBA FINALS.*$",
                @"\(2 HOURS\)",
                @"\(REBROADCAST\)", @"\(ENCORE\)", @"ENCORE ", @"\(RERUN\)", @"\(TENT\)",
                @"\(M-F\)",
                @"SIMULCAST",
            };

            var result = name;

            foreach (var pattern in filterPatterns)
            {
                try
                {
                    result = Regex.Replace(result, pattern, "", RegexOptions.IgnoreCase);
                }
                catch(Exception e)
                {
                    result = result;
                    Debug.WriteLine(e);
                }
            }

            return result;
        }

        private string _ApplyReplacement(string name)
        {
            var replacements = new Dictionary<string, string>()
            {
                {@"NWS", "NEWS"},
                {@"/W ", "WITH "},
                {@"W/ ", "WITH " },
                {@"FT\.", "FORT"},
                {@"MORN ", "MORNING "},
                {@"WKND", "WEEKEND"},
                {@"MINUT(\s|$)", "MINUTE" },
                {@"MRN", "MORNING" },
                {@"MORN(\s|$)", "MORNING" },
                {@"(^|\s)GD ", "GOOD " },
                {@"^GM ", "GOOD MORNING " },
                {@"INTL", "INTERNATIONAL" },
                {@"WMN", "WOMEN" }
            };

            var result = name;

            foreach(var replacement in replacements)
            {
                result = Regex.Replace(result, replacement.Key, replacement.Value, RegexOptions.IgnoreCase);
            }

            return result;


        }
    }
}
