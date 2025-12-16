using CustomWPFControls.ViewModels;

namespace CustomWPFControls.Tests.Testing
{
    /// <summary>
    /// Test-ViewModel für Unit/Integration-Tests.
    /// </summary>
    public class TestViewModel : ViewModelBase<TestModel>
    {
        private bool _isSelected;
        private bool _isExpanded;

        public TestViewModel(TestModel model) : base(model)
        {
        }

        // Domain-Properties (delegiert an Model)
        public int Id => Model.Id;
        public string Name => Model.Name;
        public string Description => Model.Description;

        // UI-Properties mit manuellem PropertyChanged für Tests
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        // Test-Hilfsproperty zum Tracken von Dispose-Aufrufen
        public bool IsDisposed { get; private set; }

        ~TestViewModel()
        {
            IsDisposed = true;
        }
    }
}
