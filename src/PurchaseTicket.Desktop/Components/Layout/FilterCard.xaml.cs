using System.Windows;
using System.Windows.Controls;

namespace PurchaseTicket.Desktop.Components.Layout;

public partial class FilterCard : UserControl
{
    // Campos

    public static readonly DependencyProperty FilterContentProperty =
        DependencyProperty.Register(
            nameof(FilterContent),
            typeof(object),
            typeof(FilterCard),
            new PropertyMetadata(null));

    // Constructor

    public FilterCard()
    {
        InitializeComponent();
    }

    // Propiedades

    public object? FilterContent
    {
        get => GetValue(FilterContentProperty);
        set => SetValue(FilterContentProperty, value);
    }
}
