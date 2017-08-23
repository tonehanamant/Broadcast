using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Common.Services.Extensions
{
    public static class MeastroExtensions
    {

        public static bool IsNullOrEmpty<T>(this List<T> lst)
        {
            if (lst == null || lst.Count == 0)
                return true;

            return false;
        }

        public static T Single<T>(this IQueryable<T> lst, string noMatchFoundErrorMessage)
        {
            try
            {
               return lst.Single();
            }
            catch (InvalidOperationException ex)
            {
                if (ex != null && ex.Message.Contains("Sequence contains no"))
                    throw new Exception(noMatchFoundErrorMessage);

                throw;
            }
        }

        public static T Single<T>(this IQueryable<T> lst, Expression<Func<T, bool>> predicate, string noMatchFoundErrorMessage)
        {
            try
            {
                return lst.Single(predicate);
            }
            catch (InvalidOperationException ex)
            {
                if (ex != null && ex.Message.Contains("Sequence contains no"))
                    throw new Exception(noMatchFoundErrorMessage);

                throw;
            }
        }

        public static T Single<T>(this IEnumerable<T> lst, string noMatchFoundErrorMessage)
        {
            try
            {
                return lst.Single();
            }
            catch (InvalidOperationException ex)
            {
                if (ex != null && ex.Message.Contains("Sequence contains no"))
                    throw new Exception(noMatchFoundErrorMessage);

                throw;
            }
        }

        public static T Single<T>(this IEnumerable<T> lst, Func<T, bool> predicate, string noMatchFoundErrorMessage)
        {
            try
            {
                return lst.Single(predicate);
            }
            catch (InvalidOperationException ex)
            {
                if (ex != null && ex.Message.Contains("Sequence contains no"))
                    throw new Exception(noMatchFoundErrorMessage);

                throw;
            }
        }

        public static T SingleOrDefault<T>(this IEnumerable<T> lst, string moreThanOneMatchFoundErrorMessage)
        {
            try
            {
                return lst.SingleOrDefault();
            }
            catch (InvalidOperationException ex)
            {
                if (ex != null && ex.Message.Contains("Sequence contains more than one"))
                    throw new Exception(moreThanOneMatchFoundErrorMessage);

                throw;
            }
        }

        public static List<T> ToList<T>(this IDataReader dr)
        {
            var lst = new List<T>();
            T obj = Activator.CreateInstance<T>();
            var properties = obj.GetType().GetProperties().ToList();
            var matchingProperties = new List<PropertyInfo>();
            for (int index = 0; index < dr.FieldCount; index++)
            {
                matchingProperties.Add(properties.Single(x => x.Name == dr.GetName(index)));
            }

            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                lst.Add(obj);
                foreach (PropertyInfo prop in matchingProperties)
                {
                    if (!object.Equals(dr[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, dr[prop.Name], null);
                    }
                }
            }
            return lst;
        }

    }
}
