using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PurchaseTicket.Desktop.Components.Navigation;

public partial class SidebarNavItem : UserControl
{
    // Campos

    // Texto de la opcion
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(SidebarNavItem),
            new PropertyMetadata(string.Empty));

    // Geometria del icono
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(Geometry),
            typeof(SidebarNavItem),
            new PropertyMetadata(null));

    // Comando de navegacion
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(SidebarNavItem),
            new PropertyMetadata(null));

    // Parametro del comando; en el menu es la opcion misma
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(SidebarNavItem),
            new PropertyMetadata(null));

    // Indica si la opcion esta seleccionada
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(SidebarNavItem),
            new PropertyMetadata(false));

    // Constructor

    public SidebarNavItem()
    {
        InitializeComponent();
    }

    // Propiedades

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Geometry? Icon
    {
        get => (Geometry?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
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

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    // Metodos privados

    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationButton.SetCurrentValue(
            System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty,
            IsSelected);
    }
}
