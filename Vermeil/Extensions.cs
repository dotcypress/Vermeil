#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

#endregion

namespace Vermeil
{
    public static class Extensions
    {
        #region Expressions

        public static string GetPropertyName<T>(this Expression<Func<T>> property)
        {
            var lambda = (LambdaExpression) property;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression) lambda.Body;
                memberExpression = (MemberExpression) unaryExpression.Operand;
            }
            else
            {
                memberExpression = (MemberExpression) lambda.Body;
            }
            return memberExpression.Member.Name;
        }

        public static string GetPropertyName<T, T1>(this Expression<Func<T, T1>> property)
        {
            var lambda = (LambdaExpression) property;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression) lambda.Body;
                memberExpression = (MemberExpression) unaryExpression.Operand;
            }
            else
            {
                memberExpression = (MemberExpression) lambda.Body;
            }
            return memberExpression.Member.Name;
        }

        #endregion

        #region DateTime

        public static DateTime ConvertFromUnixTimestamp(this double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp).ToLocalTime();
        }

        public static double ConvertToUnixTimestamp(this DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Math.Ceiling((date.ToUniversalTime() - origin).TotalSeconds);
        }

        #endregion

        #region Visual tree

        public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject node)
        {
            var parent = VisualTreeHelper.GetParent(node);
            while (parent != null)
            {
                yield return parent;
                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        public static DependencyObject FindAncestor(this DependencyObject target, Type ancestorType)
        {
            return target.GetAncestors().FirstOrDefault(ancestorType.IsInstanceOfType);
        }

        #endregion

        #region Enums

        public static IEnumerable<string> GetEnumNames<T>() where T : struct
        {
            var type = typeof (T);
            if (!type.IsEnum)
            {
                throw new ArgumentException(String.Format("Type '{0}' is not an enum", type.Name));
            }
            var names = type.GetFields().Where(field => field.IsLiteral).Select(field => field.Name);
            return names;
        }

        public static IEnumerable<T> GetEnumValues<T>() where T : struct
        {
            var type = typeof (T);
            if (!type.IsEnum)
            {
                throw new ArgumentException(String.Format("Type '{0}' is not an enum", type.Name));
            }
            var fields = type.GetFields().Where(field => field.IsLiteral).Select(field => (T) field.GetValue(type));
            return fields;
        }

        #endregion
        
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

        public static string Ellipsize(this string input, int maxLenght)
        {
            if (!string.IsNullOrWhiteSpace(input) && input.Length > maxLenght)
            {
                return input.Substring(0, maxLenght - 1) + "...";
            }
            return input;
        }

        public static string UppercaseFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }
        
        public static Version GetAppVersion()
        {
            try
            {
                var doc = XDocument.Load("WMAppManifest.xml");
                var xAttribute = doc.Descendants("App").First().Attribute("Version");
                if (xAttribute != null)
                {
                    var version = xAttribute.Value;
                    if (!string.IsNullOrEmpty(version))
                    {
                        Version result;
                        if (Version.TryParse(version, out result))
                        {
                            return result;
                        }
                    }
                }
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }
            return default(Version);
        }
        
        internal static int CombineHashCodes(int hash, int anotherHash)
        {
            return (hash << 5) + hash ^ anotherHash;
        }
    }
}