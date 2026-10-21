using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PurchaseTicket.Desktop.ViewModels.VehicleWeighing;

namespace PurchaseTicket.Desktop.Components.Forms;

public partial class WeightCaptureField : UserControl
{
    // Campos

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(WeightCaptureField),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.Register(
            nameof(IsRequired),
            typeof(bool),
            typeof(WeightCaptureField),
            new PropertyMetadata(true));

    public static readonly DependencyProperty CaptureProperty =
        DependencyProperty.Register(
            nameof(Capture),
            typeof(WeightCaptureViewModel),
            typeof(WeightCaptureField),
            new PropertyMetadata(null));

    // Constructor

    public WeightCaptureField()
    {
        InitializeComponent();
    }

    // Propiedades

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // Un peso casi siempre es obligatorio, por eso inicia en true
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    // Estado y comandos de la captura
    public WeightCaptureViewModel? Capture
    {
        get => (WeightCaptureViewModel?)GetValue(CaptureProperty);
        set => SetValue(CaptureProperty, value);
    }

    // Metodos privados

    // Al pasar a captura manual el cursor queda listo para escribir
    private void ManualTextBox_IsVisibleChanged(
        object sender,
        DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not true)
            return;

        // Se espera a que el campo termine de mostrarse; antes, el foco se pierde
        Dispatcher.BeginInvoke(
            () =>
            {
                ManualTextBox.Focus();
                ManualTextBox.SelectAll();
            },
            DispatcherPriority.Input);
    }
}