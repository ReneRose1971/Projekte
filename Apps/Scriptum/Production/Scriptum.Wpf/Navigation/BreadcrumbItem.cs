using System;
using PropertyChanged;

namespace Scriptum.Wpf.Navigation;

/// <summary>
/// Repräsentiert einen Eintrag in der Breadcrumb-Navigation.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class BreadcrumbItem
{
    public BreadcrumbItem(string title, Action? navigateAction = null)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        NavigateAction = navigateAction;
    }

    public string Title { get; }
    public Action? NavigateAction { get; }
    public bool IsClickable => NavigateAction != null;
}
