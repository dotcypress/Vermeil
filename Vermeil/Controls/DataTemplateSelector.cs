#region

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Vermeil.Controls
{
    public abstract class DataTemplateSelector : ContentControl
    {
        protected abstract DataTemplate GetTemplate(object item);

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            ContentTemplate = GetTemplate(newContent);
        }
    }
}
