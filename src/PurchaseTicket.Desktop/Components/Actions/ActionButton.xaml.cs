using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PurchaseTicket.Desktop.Components.Actions;

// Boton con texto e icono opcional para ventanas y vistas.
// En las filas de una tabla con solo icono se usa RowActionButton
public partial class ActionButton : UserControl
{
    // Campos

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(ActionButton),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(Geometry),
            typeof(ActionButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(
            nameof(Variant),
            typeof(ActionButtonVariant),
            typeof(ActionButton),
            new PropertyMetadata(ActionButtonVariant.Simple));

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(
            nameof(IsCompact),
            typeof(bool),
            typeof(ActionButton),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsDefaultProperty =
        DependencyProperty.Register(
            nameof(IsDefault),
            typeof(bool),
            typeof(ActionButton),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsCancelProperty =
        DependencyProperty.Register(
            nameof(IsCancel),
            typeof(bool),
            typeof(ActionButton),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(ActionButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(ActionButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TrailingContentProperty =
        DependencyProperty.Register(
            nameof(TrailingContent),
            typeof(object),
            typeof(ActionButton),
            new PropertyMetadata(null));

    // Constructor

    public ActionButton()
    {
        InitializeComponent();
    }

    // Propiedades

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // Un Geometry de Icons.xaml, como EditIcon o PrintIcon
    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public ActionButtonVariant Variant
    {
        get => (ActionButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // Mas bajo y sin ancho minimo, para las filas de una tabla
    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    // Se activa con Enter
    public bool IsDefault
    {
        get => (bool)GetValue(IsDefaultProperty);
        set => SetValue(IsDefaultProperty, value);
    }

    // Se activa con Esc
    public bool IsCancel
    {
        get => (bool)GetValue(IsCancelProperty);
        set => SetValue(IsCancelProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public object? TrailingContent
    {
        get => GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }
}
