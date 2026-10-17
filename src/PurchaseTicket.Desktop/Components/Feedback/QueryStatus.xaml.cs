using System.Windows;
using System.Windows.Controls;

namespace PurchaseTicket.Desktop.Components.Feedback;

public partial class QueryStatus : UserControl
{
    public QueryStatus()
    {
        InitializeComponent();
    }

    // Estado de carga
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(QueryStatus),
            new PropertyMetadata(false));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    // Mensaje de carga
    public static readonly DependencyProperty LoadingMessageProperty =
        DependencyProperty.Register(
            nameof(LoadingMessage),
            typeof(string),
            typeof(QueryStatus),
            new PropertyMetadata("Cargando información..."));

    public string LoadingMessage
    {
        get => (string)GetValue(LoadingMessageProperty);
        set => SetValue(LoadingMessageProperty, value);
    }

    // Mensaje de error
    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.Register(
            nameof(ErrorMessage),
            typeof(string),
            typeof(QueryStatus),
            new PropertyMetadata(string.Empty, OnErrorMessageChanged));

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    // Indica si existe un mensaje de error
    public static readonly DependencyPropertyKey HasErrorPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasError),
            typeof(bool),
            typeof(QueryStatus),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasErrorProperty =
        HasErrorPropertyKey.DependencyProperty;

    public bool HasError =>
        (bool)GetValue(HasErrorProperty);

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