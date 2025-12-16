using System;

namespace CustomWPFControls.Factories
{
    /// <summary>
    /// Factory für die Erstellung von ViewModels aus Models.
    /// Ermöglicht DI-basierte Konstruktion mit automatischer Dependency-Auflösung.
    /// </summary>
    /// <typeparam name="TModel">Model-Typ (Domain-Objekt).</typeparam>
    /// <typeparam name="TViewModel">ViewModel-Typ (UI-Wrapper).</typeparam>
    public interface IViewModelFactory<in TModel, out TViewModel>
        where TModel : class
        where TViewModel : class
    {
        /// <summary>
        /// Erstellt ein ViewModel für das gegebene Model.
        /// </summary>
        /// <param name="model">Das Model, für das ein ViewModel erstellt werden soll.</param>
        /// <returns>Neues ViewModel-Instanz.</returns>
        TViewModel Create(TModel model);
    }
}
