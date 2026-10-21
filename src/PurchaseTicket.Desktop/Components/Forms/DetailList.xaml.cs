using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace PurchaseTicket.Desktop.Components.Forms;

public partial class DetailList : UserControl
{
    // Campos

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(
            nameof(Items),
            typeof(IEnumerable),
            typeof(DetailList),
            new PropertyMetadata(null));

    // Constructor

    public DetailList()
    {
        InitializeComponent();
    }

    // Propiedades

    // Normalmente una lista de DetailItem
    public IEnumerable? Items
    {
        get => (IEnumerable?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
}