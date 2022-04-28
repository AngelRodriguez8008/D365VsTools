﻿using System;
using System.Linq;
using System.Reflection;

namespace D365VsTools.CodeGenerator.Model
{
    public static class CrmAttributeExtensions
    {
        public static string MergeCode(this Attribute attribute)
        {
            var type = attribute.GetType();

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            var values =
                properties.ToDictionary(
                    p => p.Name,
                    p => p.GetValue(attribute, new object[0]));

            var typeName = type.Name;

            if (typeName.EndsWith("Attribute"))
                typeName = typeName.Substring(0, typeName.Length - 9);

            var valuesString = values.Where(v =>
                !(v.Value == null ||
                  Equals(v.Value, "") ||
                  v.Value.Equals(GetDefaultValue(v.Value.GetType())))).Select(v =>
                $"{v.Key} = {FormatValue(v.Value)}").ToArray();

            return
                $"[{typeName}({string.Join(", ", valuesString)})]";
        }

        private static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            else
                return null;
        }

        private static string FormatValue(object value)
        {
            if (value.GetType() == typeof(bool))
                return (bool)value ? "true" : "false";

            if (value.GetType() == typeof(string))
                return $"\"{value}\"";

            return value.ToString();
        }
    }
}
