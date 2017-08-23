using log4net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Tam.Maestro.Common
{
    public static class ReflectionHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ReflectionHelper));
        public static string GetParametersAsString(MethodBase method, params object[] values)
        {
            try
            {
                ParameterInfo[] parms = method.GetParameters();
                object[] namevalues = new object[2 * parms.Length];

                string text = method.Name +  "(";
                for (int i = 0, j = 0; i < parms.Length; i++, j += 2)
                {
                    text += "{" + j + "}={" + (j + 1) + "}, ";
                    namevalues[j] = parms[i].Name;
                    if (i < values.Length) namevalues[j + 1] = values[i];
                }
                if (text.EndsWith(", "))
                    text = text.Substring(0, text.Length - 2); // get rid of the last comma
                text += ")";
                return string.Format(text, namevalues);
            }
            catch(Exception ex)
            {
                log.Warn(ex);
                return "Error Getting Parameters as String";
            }
        }

        public static string GetPublicPropertiesAsString(object obj)
        {
            try
            {
                var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                string text = props.Aggregate("(", (current, prop) => current + ("{" + prop.Name + "}={" + prop.GetValue(obj, null) + "}, "));
                if(text.Length > 2)
                    text = text.Substring(0, text.Length - 2); // get rid of the last comma
                text += ")";
                return text;
            }
            catch (Exception ex)
            {
                log.Warn(ex);
                return "Error getting public properties as string";
            }
        }

        private static string GetCustomDescription(object objEnum)
        {
            var fi = objEnum.GetType().GetField(objEnum.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : objEnum.ToString();
        }

        public static string Description(this Enum value)
        {
            return GetCustomDescription(value);
        }

        public static object GetProperty(object pObject, string pProperty)
        {
            Type lType = pObject.GetType();
            PropertyInfo lPropertyInfo = lType.GetProperty(pProperty);
            return lPropertyInfo.GetValue(pObject, null);
        }
    }
}
