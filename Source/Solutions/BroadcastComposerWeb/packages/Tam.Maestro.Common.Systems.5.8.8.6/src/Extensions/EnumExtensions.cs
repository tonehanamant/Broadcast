using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Common.Services.Extensions
{
    public static class EnumExtensions
    {
        public static List<LookupDto> ToLookupDtoList<T>() // where T : struct, IComparable, IFormattable, IConvertible
        {
            var list = new List<LookupDto>();

            foreach (var item in Enum.GetValues(typeof(T)).Cast<T>())
            {
                var enumVal = Enum.Parse(typeof (T), item.ToString());
                var fieldType = (typeof (T)).GetField(enumVal.ToString());
                var attribute =
                    Attribute.GetCustomAttribute(fieldType, typeof (DescriptionAttribute)) as DescriptionAttribute;
                string name;
                if (attribute != null)
                {
                    name = attribute.Description;
                }
                else
                {
                    name = Enum.GetName(typeof (T), enumVal);
                }

                list.Add(new LookupDto()
                {
                    Id = (int) enumVal,
                    Display = name
                });
            }

            return list;
        }
    }
}
