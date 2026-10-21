using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace PurchaseTicket.Desktop.Components.Forms;

public partial class CalculationBreakdown : UserControl
{
    // Campos

    public static readonly DependencyProperty StepsProperty =
        DependencyProperty.Register(
            nameof(Steps),
            typeof(IEnumerable),
            typeof(CalculationBreakdown),
            new PropertyMetadata(null));

    // Constructor

    public CalculationBreakdown()
    {
        InitializeComponent();
    }

    // Propiedades

    // Normalmente una lista de BreakdownStep; el ultimo suele ser el resultado
    public IEnumerable? Steps
    {
        get => (IEnumerable?)GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }
}