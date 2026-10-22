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

    // Un paso por renglon en lugar de una sola fila; para mas de tres pasos
    public static readonly DependencyProperty IsVerticalProperty =
        DependencyProperty.Register(
            nameof(IsVertical),
            typeof(bool),
            typeof(CalculationBreakdown),
            new PropertyMetadata(false));

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

    public bool IsVertical
    {
        get => (bool)GetValue(IsVerticalProperty);
        set => SetValue(IsVerticalProperty, value);
    }
}