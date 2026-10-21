using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PurchaseTicket.Desktop.Components.Forms;

public partial class FormActions : UserControl
{
    // Campos

    // Texto del boton principal
    public static readonly DependencyProperty SaveButtonTextProperty =
        DependencyProperty.Register(
            nameof(SaveButtonText),
            typeof(string),
            typeof(FormActions),
            new PropertyMetadata("Guardar"));

    // Comando para guardar
    public static readonly DependencyProperty SaveCommandProperty =
        DependencyProperty.Register(
            nameof(SaveCommand),
            typeof(ICommand),
            typeof(FormActions),
            new PropertyMetadata(null));

    // Comando para cancelar
    public static readonly DependencyProperty CancelCommandProperty =
        DependencyProperty.Register(
            nameof(CancelCommand),
            typeof(ICommand),
            typeof(FormActions),
            new PropertyMetadata(null));

    // Indica si se esta guardando el formulario
    public static readonly DependencyProperty IsSavingProperty =
        DependencyProperty.Register(
            nameof(IsSaving),
            typeof(bool),
            typeof(FormActions),
            new PropertyMetadata(false));

    // Constructor

    public FormActions()
    {
        InitializeComponent();
    }

    // Propiedades

    public string SaveButtonText
    {
        get => (string)GetValue(SaveButtonTextProperty);
        set => SetValue(SaveButtonTextProperty, value);
    }

    public ICommand? SaveCommand
    {
        get => (ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public bool IsSaving
    {
        get => (bool)GetValue(IsSavingProperty);
        set => SetValue(IsSavingProperty, value);
    }
}
