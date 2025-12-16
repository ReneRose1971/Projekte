using System.Windows;
using SolutionBundler.WPF.ViewModels;

namespace SolutionBundler.WPF;

/// <summary>
/// MainWindow mit Split-View: ProjectListEditorView (links) und LogOutputView (rechts).
/// </summary>
public partial class MainWindowWithSplitView : Window
{
    public MainWindowWithSplitView(ProjectListEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
