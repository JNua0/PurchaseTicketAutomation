using System.Windows;
using System.Windows.Controls;

namespace PurchaseTicket.Desktop.Components.Layout;

public partial class PageHeader : UserControl
{
    // Campos

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(PageHeader),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(PageHeader),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ActionsProperty =
        DependencyProperty.Register(
            nameof(Actions),
            typeof(object),
            typeof(PageHeader),
            new PropertyMetadata(null));

    // Constructor

    public PageHeader()
    {
        InitializeComponent();
    }

    // Propiedades

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }
}