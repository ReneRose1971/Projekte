using System;
using Microsoft.Extensions.DependencyInjection;

namespace CustomWPFControls.Factories
{
    /// <summary>
    /// Generische Factory, die ViewModels via ActivatorUtilities erstellt.
    /// Constructor des ViewModels muss (TModel model, ...) akzeptieren.
    /// </summary>
    public sealed class ViewModelFactory<TModel, TViewModel> : IViewModelFactory<TModel, TViewModel>
        where TModel : class
        where TViewModel : class
    {
        private readonly IServiceProvider _serviceProvider;

        public ViewModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public TViewModel Create(TModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                return ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider, model);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Fehler beim Erstellen von {typeof(TViewModel).Name} für Model {typeof(TModel).Name}. " +
                    $"Stellen Sie sicher, dass der ViewModel-Constructor (TModel model, ...) definiert ist.",
                    ex);
            }
        }
    }
}
