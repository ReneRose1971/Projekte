namespace Scriptum.Wpf.Navigation;

/// <summary>
/// Repräsentiert einen View-State für die Back-Navigation.
/// </summary>
internal sealed class ViewState
{
    public ViewState(object viewModel, string title, BreadcrumbItem[] breadcrumbs)
    {
        ViewModel = viewModel;
        Title = title;
        Breadcrumbs = breadcrumbs;
    }

    public object ViewModel { get; }
    public string Title { get; }
    public BreadcrumbItem[] Breadcrumbs { get; }
}
