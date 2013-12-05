#region

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

#endregion

namespace Vermeil
{
    public static class UIExtensions
    {
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

        public static T FindAncestorWithType<T>(this DependencyObject target) where T : class
        {
            return GetAncestors(target).FirstOrDefault(typeof (T).IsInstanceOfType) as T;
        }
        
        public static T FindFirstChild<T>(this DependencyObject target) where T : class
        {
            var childCount = VisualTreeHelper.GetChildrenCount(target);
            for (var i = 0; i < childCount; i++)
            {
                var dependencyObject = VisualTreeHelper.GetChild(target, i);
                var child = dependencyObject as T;
                if (child != null)
                {
                    return child;
                }
                var result = FindFirstChild<T>(dependencyObject);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        #endregion
    }
}
