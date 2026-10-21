using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PurchaseTicket.Desktop.Components.Actions;

public partial class CreateActionButton : UserControl
{
    // Campos

    // Texto que mostrara el boton
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(CreateActionButton),
            new PropertyMetadata(string.Empty));

    // Comando que ejecutara el boton
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(CreateActionButton),
            new PropertyMetadata(null));

    // Parametro opcional del comando
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(CreateActionButton),
            new PropertyMetadata(null));

    // Constructor

    public CreateActionButton()
    {
        InitializeComponent();
    }

    // Propiedades

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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
}
