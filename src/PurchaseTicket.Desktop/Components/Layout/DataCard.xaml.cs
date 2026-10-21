using System.Windows;
using System.Windows.Controls;

namespace PurchaseTicket.Desktop.Components.Layout;

public partial class DataCard : UserControl
{
    // Campos

    public static readonly DependencyProperty DataContentProperty =
        DependencyProperty.Register(
            nameof(DataContent),
            typeof(object),
            typeof(DataCard),
            new PropertyMetadata(null));

    // Constructor

    public DataCard()
    {
        InitializeComponent();
    }

    // Propiedades

    public object? DataContent
    {
        get => GetValue(DataContentProperty);
        set => SetValue(DataContentProperty, value);
    }
}