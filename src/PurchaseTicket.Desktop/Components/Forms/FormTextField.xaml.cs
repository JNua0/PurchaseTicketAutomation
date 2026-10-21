using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PurchaseTicket.Desktop.Components.Forms;

public partial class FormTextField : UserControl
{
    // Campos

    // Etiqueta del campo
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(FormTextField),
            new PropertyMetadata(string.Empty));

    // Indica si el campo es obligatorio
    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.Register(
            nameof(IsRequired),
            typeof(bool),
            typeof(FormTextField),
            new PropertyMetadata(false));

    // Indica si el cursor debe colocarse en este campo al abrir el formulario
    public static readonly DependencyProperty FocusOnLoadProperty =
        DependencyProperty.Register(
            nameof(FocusOnLoad),
            typeof(bool),
            typeof(FormTextField),
            new PropertyMetadata(false));

    // Valor del campo
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(FormTextField),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    // Longitud maxima del texto
    public static readonly DependencyProperty MaxLengthProperty =
        DependencyProperty.Register(
            nameof(MaxLength),
            typeof(int),
            typeof(FormTextField),
            new PropertyMetadata(0));

    // Mayusculas o minusculas al escribir; las placas se ven como quedaran guardadas
    public static readonly DependencyProperty CharacterCasingProperty =
        DependencyProperty.Register(
            nameof(CharacterCasing),
            typeof(CharacterCasing),
            typeof(FormTextField),
            new PropertyMetadata(CharacterCasing.Normal));

    // Constructor

    public FormTextField()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    // Propiedades

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    public bool FocusOnLoad
    {
        get => (bool)GetValue(FocusOnLoadProperty);
        set => SetValue(FocusOnLoadProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public CharacterCasing CharacterCasing
    {
        get => (CharacterCasing)GetValue(CharacterCasingProperty);
        set => SetValue(CharacterCasingProperty, value);
    }

    // Metodos privados

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!FocusOnLoad)
            return;

        // Se espera a que la ventana termine de mostrarse; antes, el foco se pierde
        Dispatcher.BeginInvoke(
            () =>
            {
                InputTextBox.Focus();

                // Al editar, el cursor queda al final del texto existente
                InputTextBox.CaretIndex = InputTextBox.Text.Length;
            },
            DispatcherPriority.Input);
    }
}