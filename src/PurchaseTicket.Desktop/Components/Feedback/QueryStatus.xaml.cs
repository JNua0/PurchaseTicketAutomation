using System.Windows;
using System.Windows.Controls;

namespace PurchaseTicket.Desktop.Components.Feedback;

public partial class QueryStatus : UserControl
{
    // Campos

    // Estado de carga
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(QueryStatus),
            new PropertyMetadata(false));

    // Mensaje de carga
    public static readonly DependencyProperty LoadingMessageProperty =
        DependencyProperty.Register(
            nameof(LoadingMessage),
            typeof(string),
            typeof(QueryStatus),
            new PropertyMetadata("Cargando información..."));

    // Mensaje de error
    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.Register(
            nameof(ErrorMessage),
            typeof(string),
            typeof(QueryStatus),
            new PropertyMetadata(string.Empty, OnErrorMessageChanged));

    // Indica si existe un mensaje de error
    public static readonly DependencyPropertyKey HasErrorPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasError),
            typeof(bool),
            typeof(QueryStatus),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasErrorProperty =
        HasErrorPropertyKey.DependencyProperty;

    // Constructor

    public QueryStatus()
    {
        InitializeComponent();
    }

    // Propiedades

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string LoadingMessage
    {
        get => (string)GetValue(LoadingMessageProperty);
        set => SetValue(LoadingMessageProperty, value);
    }

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public bool HasError =>
        (bool)GetValue(HasErrorProperty);

    // Metodos privados

    // Actualizar el estado del mensaje de error
    private static void OnErrorMessageChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not QueryStatus control)
            return;

        var message = e.NewValue as string;

        control.SetValue(
            HasErrorPropertyKey,
            !string.IsNullOrWhiteSpace(message));
    }
}