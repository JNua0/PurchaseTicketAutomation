using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PurchaseTicket.Desktop.Components.Actions;

// El texto de ayuda se asigna con la propiedad ToolTip del propio control
public partial class RowActionButton : UserControl
{
    // Campos

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(Geometry),
            typeof(RowActionButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(
            nameof(Variant),
            typeof(RowActionVariant),
            typeof(RowActionButton),
            new PropertyMetadata(RowActionVariant.Edit));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(RowActionButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(RowActionButton),
            new PropertyMetadata(null));

    // Constructor

    public RowActionButton()
    {
        InitializeComponent();
    }

    // Propiedades

    // Un Geometry de Icons.xaml, como EditIcon o PrintIcon
    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public RowActionVariant Variant
    {
        get => (RowActionVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    // Normalmente la fila: CommandParameter="{Binding}"
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}