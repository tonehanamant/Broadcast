using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Services.Broadcast.IntegrationTests.Helpers
{
    public static class ReflectionTestHelper
    {
        public static IList CreateGenericList(Type t)
        {
            Type genericListType = typeof(List<>).MakeGenericType(t);
            var list = (IList)Activator.CreateInstance(genericListType);
            return list;
        }

        public static object CreateInstanceAndSetProperty(Type t, string propertyName, int testValue)
        {
            var instance = Activator.CreateInstance(t);
            t.GetProperty(propertyName).SetValue(instance, testValue);
            return instance;
        }

        public static MethodInfo GetGenericMethod(Type t, Type methodType, string methodName)
        {
            MethodInfo method = methodType.GetMethod(methodName);
            MethodInfo genericMethod = method.MakeGenericMethod(t);
            return genericMethod;
        }
    }
}