using System.Windows;
using System.Windows.Controls;

namespace PurchaseTicket.Desktop.Components.Filters;

public partial class StatusFilter : UserControl
{
    public StatusFilter()
    {
        InitializeComponent();
    }

    // Etiqueta del filtro
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(StatusFilter),
            new PropertyMetadata("Estado"));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // Estado seleccionado
    public static readonly DependencyProperty SelectedStatusProperty =
        DependencyProperty.Register(
            nameof(SelectedStatus),
            typeof(bool?),
            typeof(StatusFilter),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool? SelectedStatus
    {
        get => (bool?)GetValue(SelectedStatusProperty);
        set => SetValue(SelectedStatusProperty, value);
    }

    // Estado de carga
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(StatusFilter),
            new PropertyMetadata(false));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
}
