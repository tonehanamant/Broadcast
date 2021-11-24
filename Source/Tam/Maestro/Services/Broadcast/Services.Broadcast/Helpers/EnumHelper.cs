using Services.Broadcast.Entities.Enums;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Services.Broadcast.Helpers
{
    public static class EnumHelper
    {
        public static string GetFileDetailProblemDescription(FileDetailProblemTypeEnum problemType)
        {
            switch (problemType)
            {
                case FileDetailProblemTypeEnum.UnlinkedIsci:
                    return "Not in system";
                case FileDetailProblemTypeEnum.UnmarriedOnMultipleContracts:
                    return "Multiple Proposals";
                case FileDetailProblemTypeEnum.MarriedAndUnmarried:
                    return "Married and Unmarried";
                case FileDetailProblemTypeEnum.UnmatchedSpotLength:
                    return "Unmatched Spot length";
                default:
                    return null;
            }
        }
        public static bool IsCustomDaypart(string daypartType)
        {
            switch (daypartType)
            {               
                case "Sports":
                    return true;
                default:
                    return false;
            }
        }

        public static string GetDescriptionAttribute<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }

        public static T GetEnumValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            FieldInfo[] fields = type.GetFields();
            var field = fields.SelectMany(f => f.GetCustomAttributes(typeof(DescriptionAttribute), false), (f, a) => new { Field = f, Att = a })
                                .Where(a => ((DescriptionAttribute)a.Att).Description.Equals(description, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
        }

        public static List<InventorySourceEnum> GetProprietaryInventorySources()
        {
            var inventorySources = Enum.GetValues(typeof(InventorySourceEnum)).Cast<InventorySourceEnum>().ToList();

            inventorySources.Remove(InventorySourceEnum.Blank);
            inventorySources.Remove(InventorySourceEnum.Assembly);
            inventorySources.Remove(InventorySourceEnum.OpenMarket);

            return inventorySources;
        }

        /// <summary>
        /// Gets the enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="candidate">The candidate.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Given status of {candidate} is an invalid {typeof(T).FullName}.</exception>
        public static T GetEnum<T>(int candidate)
        {
            if (typeof(T).IsEnum == false)
            {
                throw new InvalidOperationException("The type must be an Enum.");
            }

            if (Enum.IsDefined(typeof(T), candidate) == false)
            {
                throw new InvalidOperationException($"Given status of {candidate} is an invalid {typeof(T).FullName}.");
            }
            var result = (T)Enum.Parse(typeof(T), candidate.ToString());
            return result;
        }

        public static bool IsDefined<T>(T value)
        {
            return Enum.IsDefined(typeof(T), value);
        }

        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
