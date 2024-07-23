using BFBMX.Service.Models;
using System.Windows;
using System.Windows.Controls;

namespace BFBMX.Desktop.Helpers
{
    public class DiscoveredFilesTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement? element = container as FrameworkElement;

            if (element is not null && item is not null && item is DiscoveredFileModel)
            {
                DiscoveredFileModel? dfmItem = item as DiscoveredFileModel;

                if (dfmItem is not null)
                {
                    if (dfmItem.HasWarningFlag)
                    {
                        return element.FindResource("BibWarning") as DataTemplate;
                    }
                    else
                    {
                        return element.FindResource("BibNominal") as DataTemplate;
                    }
                }
            }

            return null;
        }
    }
}
