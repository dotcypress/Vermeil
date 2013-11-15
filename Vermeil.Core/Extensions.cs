#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;

#endregion

namespace Vermeil.Core
{
    public delegate void PropertyChangedCallback<T>(T sender, DependencyPropertyChangedEventArgs e) where T : DependencyObject;
    public static class Extensions
    {
        #region Expressions

        public static string GetPropertyName<T>(this Expression<Func<T>> property)
        {
            var lambda = (LambdaExpression) property;
            MemberExpression memberExpression;
            var body = lambda.Body as UnaryExpression;
            if (body != null)
            {
                var unaryExpression = body;
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
            var body = lambda.Body as UnaryExpression;
            if (body != null)
            {
                var unaryExpression = body;
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

        #region Collections

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

        #endregion

        #region Strings

        public static string Ellipsize(this string input, int maxLenght)
        {
            if (!string.IsNullOrWhiteSpace(input) && input.Length > maxLenght)
            {
                return input.Substring(0, maxLenght - 1) + "...";
            }
            return input;
        }

        public static string Capitalize(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        #endregion

        #region Helpers

        public static TResult With<TInput, TResult>(this TInput input, Func<TInput, TResult> evaluator) where TResult : class where TInput : class
        {
            return input == null ? null : evaluator(input);
        }

        public static TResult Return<TInput, TResult>(this TInput input, Func<TInput, TResult> evaluator, TResult failureValue) where TInput : class
        {
            return input == null ? failureValue : evaluator(input);
        }

        public static TInput If<TInput>(this TInput input, Func<TInput, bool> evaluator) where TInput : class
        {
            if (input == null)
            {
                return null;
            }
            return evaluator(input) ? input : null;
        }

        public static TInput Unless<TInput>(this TInput input, Func<TInput, bool> evaluator) where TInput : class
        {
            if (input == null)
            {
                return null;
            }
            return evaluator(input) ? null : input;
        }

        public static TInput Do<TInput>(this TInput input, Action<TInput> action) where TInput : class
        {
            if (input == null)
            {
                return null;
            }
            action(input);
            return input;
        }

        #endregion

        public static DependencyProperty Register<TProperty, TOwner>(string name,
            TProperty defaultValue = default(TProperty),
            PropertyChangedCallback<TOwner> callback = null) where TOwner : DependencyObject
        {
            return RegisterImpl(false, name, defaultValue, callback);
        }

        public static DependencyProperty RegisterAttached<TProperty, TOwner>(string name,
            TProperty defaultValue = default(TProperty),
            PropertyChangedCallback<TOwner> callback = null) where TOwner : DependencyObject
        {
            return RegisterImpl(true, name, defaultValue, callback);
        }

        private static DependencyProperty RegisterImpl<TProperty, TOwner>(bool isAttached,
            string name,
            TProperty defaultValue = default(TProperty),
            PropertyChangedCallback<TOwner> callback = null) where TOwner : DependencyObject
        {
            var callbackWrapper = callback != null
                ? (sender, e) => callback((TOwner)sender, e)
                : (PropertyChangedCallback)null;

            return isAttached
                ? DependencyProperty.RegisterAttached(name,
                    typeof(TProperty),
                    typeof(TOwner),
                    new PropertyMetadata(defaultValue, callbackWrapper))
                : DependencyProperty.Register(name,
                    typeof(TProperty),
                    typeof(TOwner),
                    new PropertyMetadata(defaultValue, callbackWrapper));
        }
    
    }
}
