using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PurchaseTicket.Desktop.Components.Feedback;

public partial class Toast : UserControl
{
    // Campos

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(Toast),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(
            nameof(Kind),
            typeof(StatusKind),
            typeof(Toast),
            new PropertyMetadata(StatusKind.Success));

    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(
            nameof(CloseCommand),
            typeof(ICommand),
            typeof(Toast),
            new PropertyMetadata(null));

    // Constructor

    public Toast()
    {
        InitializeComponent();
    }

    // Propiedades

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public StatusKind Kind
    {
        get => (StatusKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }
}