
using System.Windows;
using System.Windows.Controls;

namespace PurchaseTicket.Desktop.Components.Layout;

public partial class FilterCard : UserControl
{
    public FilterCard()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty FilterContentProperty =
        DependencyProperty.Register(
            nameof(FilterContent),
            typeof(object),
            typeof(FilterCard),
            new PropertyMetadata(null));

    public object? FilterContent
    {
        get => GetValue(FilterContentProperty);
        set => SetValue(FilterContentProperty, value);
    }
}
