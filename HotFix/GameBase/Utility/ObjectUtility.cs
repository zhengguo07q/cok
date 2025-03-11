using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Linq;

namespace GameBase.Utility
{
    public static class ObjectUtility
    {
        public static string DumpObject(object obj, int indentLevel = 0)
        {
            if (obj == null) return "null";

            var sb = new StringBuilder();
            var indent = new string(' ', indentLevel * 4);
            var type = obj.GetType();

            sb.AppendLine($"{indent}{type.Name} {{");

            // 获取属性
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(obj);
                    sb.AppendLine($"{indent}    {prop.Name}: {FormatValue(value, indentLevel + 1)}");
                }
                catch (Exception)
                {
                    sb.AppendLine($"{indent}    {prop.Name}: <Error reading value>");
                }
            }

            // 获取字段
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                try
                {
                    var value = field.GetValue(obj);
                    sb.AppendLine($"{indent}    {field.Name}: {FormatValue(value, indentLevel + 1)}");
                }
                catch (Exception)
                {
                    sb.AppendLine($"{indent}    {field.Name}: <Error reading value>");
                }
            }

            sb.Append($"{indent}}}");
            return sb.ToString();
        }

        private static string FormatValue(object value, int indentLevel)
        {
            if (value == null) return "null";

            // 处理基本类型
            if (value is string) return $"\"{value}\"";
            if (value is DateTime dateTime) return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            if (value.GetType().IsPrimitive) return value.ToString();

            // 处理集合类型
            if (value is IEnumerable enumerable && !(value is string))
            {
                var items = enumerable.Cast<object>()
                    .Select(x => FormatValue(x, indentLevel))
                    .ToList();
                return $"[{string.Join(", ", items)}]";
            }

            // 处理复杂对象（递归）
            if (!value.GetType().Namespace?.StartsWith("System") ?? false)
            {
                return DumpObject(value, indentLevel);
            }

            return value.ToString();
        }
    }
}