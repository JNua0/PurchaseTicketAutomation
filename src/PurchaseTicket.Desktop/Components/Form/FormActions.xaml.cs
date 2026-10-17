
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PurchaseTicket.Desktop.Components.Forms;

public partial class FormActions : UserControl
{
    public FormActions()
    {
        InitializeComponent();
    }

    // Texto del boton principal
    public static readonly DependencyProperty SaveButtonTextProperty =
        DependencyProperty.Register(
            nameof(SaveButtonText),
            typeof(string),
            typeof(FormActions),
            new PropertyMetadata("Guardar"));

    public string SaveButtonText
    {
        get => (string)GetValue(SaveButtonTextProperty);
        set => SetValue(SaveButtonTextProperty, value);
    }

    // Comando para guardar
    public static readonly DependencyProperty SaveCommandProperty =
        DependencyProperty.Register(
            nameof(SaveCommand),
            typeof(ICommand),
            typeof(FormActions),
            new PropertyMetadata(null));

    public ICommand? SaveCommand
    {
        get => (ICommand?)GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    // Comando para cancelar
    public static readonly DependencyProperty CancelCommandProperty =
        DependencyProperty.Register(
            nameof(CancelCommand),
            typeof(ICommand),
            typeof(FormActions),
            new PropertyMetadata(null));

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    // Indica si se esta guardando el formulario
    public static readonly DependencyProperty IsSavingProperty =
        DependencyProperty.Register(
            nameof(IsSaving),
            typeof(bool),
            typeof(FormActions),
            new PropertyMetadata(false));

    public bool IsSaving
    {
        get => (bool)GetValue(IsSavingProperty);
        set => SetValue(IsSavingProperty, value);
    }
}
