#region

using System;
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

        public static DependencyObject FindAncestor(this DependencyObject target, Type ancestorType)
        {
            return GetAncestors(target).FirstOrDefault(ancestorType.IsInstanceOfType);
        }

        #endregion
    }
}
