#region

using System.Windows;
using Microsoft.Phone.Controls;
using Vermeil.Core;

#endregion

namespace Vermeil.MVVM
{
    public abstract class ViewModel : DependencyObject
    {
        public static readonly DependencyProperty OrientationProperty = VermeilExtensions.Register<PageOrientation, ViewModel>("Orientation", PageOrientation.None, OnOrientationChanged);

        internal bool IsCreated;

        #region Virtual members

        public FrameworkElement RootElement { get; internal set; }

        public PageOrientation Orientation
        {
            get { return (PageOrientation) GetValue(OrientationProperty); }
            internal set { SetValue(OrientationProperty, value); }
        }

        protected virtual void OnCreate()
        {
        }

        protected virtual void OnLoad()
        {
        }

        protected virtual void OnUnload()
        {
        }

        protected virtual void OnOrientationChanged()
        {
        }

        #endregion

        #region Private members

        internal void FireOnLoad()
        {
            OnLoad();
        }

        internal void FireOnUnload()
        {
            OnUnload();
        }

        internal void FireOnCreate()
        {
            OnCreate();
        }

        #endregion

        private static void OnOrientationChanged(ViewModel model, DependencyPropertyChangedEventArgs e)
        {
            model.OnOrientationChanged();
        }
    }
}
