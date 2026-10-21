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
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCounterInputChanged));

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

    // Limite que muestra el contador "18 / 35"; 0 no muestra contador. A
    // diferencia de MaxLength no corta el texto: al pegar, el operador ve
    // que sobra en lugar de perder el final sin darse cuenta
    public static readonly DependencyProperty CounterLimitProperty =
        DependencyProperty.Register(
            nameof(CounterLimit),
            typeof(int),
            typeof(FormTextField),
            new PropertyMetadata(0, OnCounterInputChanged));

    // Las calcula el propio control a partir de Text y CounterLimit
    public static readonly DependencyProperty CounterTextProperty =
        DependencyProperty.Register(
            nameof(CounterText),
            typeof(string),
            typeof(FormTextField),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsOverLimitProperty =
        DependencyProperty.Register(
            nameof(IsOverLimit),
            typeof(bool),
            typeof(FormTextField),
            new PropertyMetadata(false));

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

    public int CounterLimit
    {
        get => (int)GetValue(CounterLimitProperty);
        set => SetValue(CounterLimitProperty, value);
    }

    public string CounterText => (string)GetValue(CounterTextProperty);

    public bool IsOverLimit => (bool)GetValue(IsOverLimitProperty);

    // Metodos privados

    private static void OnCounterInputChanged(
        DependencyObject sender,
        DependencyPropertyChangedEventArgs e)
    {
        var field = (FormTextField)sender;
        int limit = field.CounterLimit;

        // Se cuenta sin los espacios de los extremos: no se imprimen
        int length = field.Text?.Trim().Length ?? 0;

        field.SetValue(
            CounterTextProperty,
            limit > 0 ? $"{length} / {limit}" : string.Empty);

        field.SetValue(
            IsOverLimitProperty,
            limit > 0 && length > limit);
    }

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