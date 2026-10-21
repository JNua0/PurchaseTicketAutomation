using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PurchaseTicket.Desktop.Components.Actions;

public partial class StatusActionButton : UserControl
{
    // Campos

    // Estado actual del registro
    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(StatusActionButton),
            new PropertyMetadata(false));

    // Comando para cambiar el estado
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(StatusActionButton),
            new PropertyMetadata(null));

    // Registro sobre el que se ejecutara el comando
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(StatusActionButton),
            new PropertyMetadata(null));

    // Icono para activar; si no se indica, se usa el generico
    public static readonly DependencyProperty ActivateIconProperty =
        DependencyProperty.Register(
            nameof(ActivateIcon),
            typeof(Geometry),
            typeof(StatusActionButton),
            new PropertyMetadata(null));

    // Icono para desactivar; si no se indica, se usa el generico
    public static readonly DependencyProperty DeactivateIconProperty =
        DependencyProperty.Register(
            nameof(DeactivateIcon),
            typeof(Geometry),
            typeof(StatusActionButton),
            new PropertyMetadata(null));

    // Constructor

    public StatusActionButton()
    {
        InitializeComponent();
    }

    // Propiedades

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
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

    public Geometry? ActivateIcon
    {
        get => (Geometry?)GetValue(ActivateIconProperty);
        set => SetValue(ActivateIconProperty, value);
    }

    public Geometry? DeactivateIcon
    {
        get => (Geometry?)GetValue(DeactivateIconProperty);
        set => SetValue(DeactivateIconProperty, value);
    }
}
