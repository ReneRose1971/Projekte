using System.Windows.Input;
using Scriptum.Core;
using Scriptum.Engine;

namespace Scriptum.Wpf.Keyboard;

/// <summary>
/// Behandelt Keyboard-Input und koordiniert Keyboard-State-Updates.
/// </summary>
public interface IKeyboardInputHandler
{
    /// <summary>
    /// Behandelt einen KeyDown-Event.
    /// </summary>
    /// <param name="e">Die Key-Event-Args.</param>
    /// <returns>True, wenn ein Input verarbeitet wurde, sonst false.</returns>
    bool HandleKeyDown(KeyEventArgs e);
    
    /// <summary>
    /// Behandelt einen KeyUp-Event.
    /// </summary>
    /// <param name="e">Die Key-Event-Args.</param>
    void HandleKeyUp(KeyEventArgs e);
}
