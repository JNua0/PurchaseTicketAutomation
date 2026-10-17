using System.Windows;
using System.Windows.Controls;

namespace PurchaseTicket.Desktop.Components.Layout;

public partial class PageLayout : UserControl
{
    public PageLayout()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty PageContentProperty =
        DependencyProperty.Register(
            nameof(PageContent),
            typeof(object),
            typeof(PageLayout),
            new PropertyMetadata(null));

    public object? PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }
}
