namespace CustomWPFControls.ViewModels
{
    /// <summary>
    /// Interface für ViewModels, die ein Domain-Model wrappen.
    /// Ermöglicht typsichere Model-Extraktion für CollectionViewModel.
    /// </summary>
    /// <typeparam name="TModel">Der Typ des gewrappten Models.</typeparam>
    public interface IViewModelWrapper<out TModel> where TModel : class
    {
        /// <summary>
        /// Das gewrappte Domain-Model.
        /// </summary>
        TModel Model { get; }
    }
}
