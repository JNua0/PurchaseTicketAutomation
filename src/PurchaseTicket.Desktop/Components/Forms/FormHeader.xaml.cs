using System.Windows;
using System.Windows.Controls;

namespace PurchaseTicket.Desktop.Components.Forms;

public partial class FormHeader : UserControl
{
    // Campos

    // Titulo del formulario
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(FormHeader),
            new PropertyMetadata(string.Empty));

    // Descripcion del formulario
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(FormHeader),
            new PropertyMetadata(string.Empty));

    // Constructor

    public FormHeader()
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
}
