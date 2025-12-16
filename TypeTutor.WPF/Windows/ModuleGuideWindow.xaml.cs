using System.Windows;
using System.Windows.Markup;

namespace TypeTutor.WPF
{
    public partial class ModuleGuideWindow : Window
    {
        public ModuleGuideViewModel ListVM { get; }

        public ModuleGuideWindow(ModuleGuideViewModel vm)
        {
            ListVM = vm;
            InitializeComponent();
            DataContext = this;
        }
    }
}
