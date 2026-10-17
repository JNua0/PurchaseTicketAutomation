using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace PurchaseTicket.Desktop.Components.Pagination;

public partial class PaginationControl : UserControl
{
    public PaginationControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty TotalCountProperty =
        DependencyProperty.Register(
            nameof(TotalCount),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(0));

    public int TotalCount
    {
        get => (int)GetValue(TotalCountProperty);
        set => SetValue(TotalCountProperty, value);
    }

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(
            nameof(CurrentPage),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(1));

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register(
            nameof(TotalPages),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(1));

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public static readonly DependencyProperty PreviousPageCommandProperty =
        DependencyProperty.Register(
            nameof(PreviousPageCommand),
            typeof(ICommand),
            typeof(PaginationControl),
            new PropertyMetadata(null));

    public ICommand? PreviousPageCommand
    {
        get => (ICommand?)GetValue(PreviousPageCommandProperty);
        set => SetValue(PreviousPageCommandProperty, value);
    }

    public static readonly DependencyProperty NextPageCommandProperty =
        DependencyProperty.Register(
            nameof(NextPageCommand),
            typeof(ICommand),
            typeof(PaginationControl),
            new PropertyMetadata(null));

    public ICommand? NextPageCommand
    {
        get => (ICommand?)GetValue(NextPageCommandProperty);
        set => SetValue(NextPageCommandProperty, value);
    }

    // Cantidad de registros por pagina
    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(
            nameof(PageSize),
            typeof(int),
            typeof(PaginationControl),
            new FrameworkPropertyMetadata(
                20,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    // Opciones disponibles de paginacion
    public static readonly DependencyProperty PageSizeOptionsProperty =
        DependencyProperty.Register(
            nameof(PageSizeOptions),
            typeof(IEnumerable<int>),
            typeof(PaginationControl),
            new PropertyMetadata(null));

    public IEnumerable<int>? PageSizeOptions
    {
        get => (IEnumerable<int>?)GetValue(PageSizeOptionsProperty);
        set => SetValue(PageSizeOptionsProperty, value);
    }

    // Comando para cambiar la cantidad de registros
    public static readonly DependencyProperty ChangePageSizeCommandProperty =
        DependencyProperty.Register(
            nameof(ChangePageSizeCommand),
            typeof(ICommand),
            typeof(PaginationControl),
            new PropertyMetadata(null));

    public ICommand? ChangePageSizeCommand
    {
        get => (ICommand?)GetValue(ChangePageSizeCommandProperty);
        set => SetValue(ChangePageSizeCommandProperty, value);
    }

    private void PageSizeComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!IsLoaded || e.AddedItems.Count == 0)
            return;

        if (ChangePageSizeCommand?.CanExecute(null) == true)
        {
            ChangePageSizeCommand.Execute(null);
        }
    }

    // Indica si se estan cargando registros
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(PaginationControl),
            new PropertyMetadata(false));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
}
