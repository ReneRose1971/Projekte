using System.Windows.Controls;
using System.Windows;

namespace TypeTutor.WPF
{
    public partial class ModuleGuideListView : UserControl
    {
        public ModuleGuideListView()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ensure DataContext.Selected is updated by binding, no-op here
        }
    }
}
