using CustomWPFControls.Factories;
using CustomWPFControls.ViewModels;

namespace SolutionBundler.WPF.ViewModels;

/// <summary>
/// Einfache ViewModelFactory-Implementierung mit Func-Delegate.
/// Verwendet keine DI für die ViewModel-Erstellung, sondern eine einfache Factory-Funktion.
/// </summary>
/// <typeparam name="TModel">Typ des Models.</typeparam>
/// <typeparam name="TViewModel">Typ des ViewModels.</typeparam>
public sealed class SimpleViewModelFactory<TModel, TViewModel> : IViewModelFactory<TModel, TViewModel>
    where TModel : class
    where TViewModel : class, IViewModelWrapper<TModel>
{
    private readonly Func<TModel, TViewModel> _factoryFunc;

    /// <summary>
    /// Erstellt eine neue SimpleViewModelFactory mit der angegebenen Factory-Funktion.
    /// </summary>
    /// <param name="factoryFunc">Funktion zum Erstellen eines ViewModels aus einem Model.</param>
    public SimpleViewModelFactory(Func<TModel, TViewModel> factoryFunc)
    {
        _factoryFunc = factoryFunc ?? throw new ArgumentNullException(nameof(factoryFunc));
    }

    /// <summary>
    /// Erstellt ein ViewModel für das angegebene Model.
    /// </summary>
    /// <param name="model">Das Model.</param>
    /// <returns>Ein neues ViewModel.</returns>
    public TViewModel Create(TModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return _factoryFunc(model);
    }
}
