using System;
using PropertyChanged;

namespace Scriptum.Wpf.Navigation;

/// <summary>
/// Repräsentiert einen Navigationseintrag in der Sidebar.
/// </summary>
[AddINotifyPropertyChangedInterface]
public sealed class NavigationItem
{
    public NavigationItem(string key, string title, Action action, string? iconGlyph = null)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Action = action ?? throw new ArgumentNullException(nameof(action));
        IconGlyph = iconGlyph;
        IsEnabled = true;
    }

    public string Key { get; }
    public string Title { get; }
    public string? IconGlyph { get; }
    public Action Action { get; }
    public bool IsEnabled { get; set; }
    public bool IsSelected { get; set; }
}
