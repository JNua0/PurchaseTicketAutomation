using PurchaseTicket.Desktop.ViewModels.Base;

namespace PurchaseTicket.Desktop.ViewModels.Navigation;

// Una opcion del menu lateral: su nombre, su icono (una clave de Icons.xaml),
// la pantalla que muestra y lo que se carga al entrar
public sealed class NavigationItem : ObservableObject
{
    // Campos

    private bool _isSelected;

    // Constructor

    public NavigationItem(
        string label,
        string iconKey,
        object viewModel,
        Func<Task>? onEnter = null)
    {
        Label = label;
        IconKey = iconKey;
        ViewModel = viewModel;
        OnEnter = onEnter;
    }

    // Propiedades

    public string Label { get; }

    public string IconKey { get; }

    public object ViewModel { get; }

    // Null si la pantalla se carga sola, como Inicio al mostrarse
    public Func<Task>? OnEnter { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
