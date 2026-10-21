using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PurchaseTicket.Desktop.Components.Filters;

public partial class SearchBox : UserControl
{
    // Propiedades de dependencia

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(SearchBox),
            new PropertyMetadata("Buscar"));

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(
            nameof(SearchText),
            typeof(string),
            typeof(SearchBox),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(SearchBox),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(
            nameof(SearchCommand),
            typeof(ICommand),
            typeof(SearchBox),
            new PropertyMetadata(null));

    // Constructor

    public SearchBox()
    {
        InitializeComponent();
    }

    // Propiedades

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    // Comando que se ejecuta al presionar Enter o al limpiar con Esc
    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    // Metodos privados

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ExecuteSearch();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape
                 && !string.IsNullOrEmpty(SearchText))
        {
            // Limpiar y volver a mostrar todos los registros
            SearchText = string.Empty;
            ExecuteSearch();
            e.Handled = true;
        }
    }

    private void ExecuteSearch()
    {
        if (SearchCommand?.CanExecute(null) == true)
            SearchCommand.Execute(null);
    }
}