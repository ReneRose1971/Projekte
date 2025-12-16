using System;
using System.Collections.Generic;
using System.Windows.Input;
using CustomWPFControls.Commands;
using DataToolKit.Abstractions.DataStores;

namespace CustomWPFControls.ViewModels
{
    /// <summary>
    /// Erweitert CollectionViewModel um Bearbeitungs-Commands (Add, Delete, Clear, Edit).
    /// Unterstützt DataStore-Integration mit bidirektionaler Synchronisation.
    /// </summary>
    /// <typeparam name="TModel">Model-Typ (Domain-Objekt).</typeparam>
    /// <typeparam name="TViewModel">ViewModel-Typ (muss IViewModelWrapper&lt;TModel&gt; implementieren).</typeparam>
    public class EditableCollectionViewModel<TModel, TViewModel> : CollectionViewModel<TModel, TViewModel>
        where TModel : class
        where TViewModel : class, IViewModelWrapper<TModel>
    {
        /// <summary>
        /// Factory-Funktion zum Erstellen neuer Models.
        /// </summary>
        public Func<TModel>? CreateModel { get; set; }

        /// <summary>
        /// Action zum Bearbeiten eines Models (z.B. Dialog öffnen).
        /// </summary>
        public Action<TModel>? EditModel { get; set; }

        /// <summary>
        /// Erstellt ein EditableCollectionViewModel mit DataStore-Integration.
        /// </summary>
        public EditableCollectionViewModel(
            IDataStore<TModel> dataStore,
            Factories.IViewModelFactory<TModel, TViewModel> viewModelFactory,
            IEqualityComparer<TModel> modelComparer)
            : base(dataStore, viewModelFactory, modelComparer)
        {
        }

        #region Commands

        private ICommand? _addCommand;
        /// <summary>
        /// Command zum Hinzufügen eines neuen Elements.
        /// </summary>
        public ICommand AddCommand => _addCommand ??= new RelayCommand(_ =>
        {
            if (CreateModel == null)
                throw new InvalidOperationException("CreateModel muss gesetzt sein.");

            var model = CreateModel();

            if (model != null)  // ← Null-Check hier!
            {
                AddModel(model);
            }
        }, _ => CreateModel != null);

        private ICommand? _deleteCommand;
        /// <summary>
        /// Command zum Löschen des ausgewählten Elements.
        /// </summary>
        public ICommand DeleteCommand => _deleteCommand ??= new RelayCommand(_ =>
        {
            if (SelectedItem != null)
            {
                RemoveViewModel(SelectedItem);
            }
        }, _ => SelectedItem != null);

        private ICommand? _clearCommand;
        /// <summary>
        /// Command zum Löschen aller Elemente.
        /// </summary>
        public ICommand ClearCommand => _clearCommand ??= new RelayCommand(_ =>
        {
            Clear();
        }, _ => Count > 0);

        private ICommand? _editCommand;
        /// <summary>
        /// Command zum Bearbeiten des ausgewählten Elements.
        /// </summary>
        public ICommand EditCommand => _editCommand ??= new RelayCommand(_ =>
        {
            if (SelectedItem != null && EditModel != null)
            {
                EditModel(SelectedItem.Model);
            }
        }, _ => SelectedItem != null && EditModel != null);

        #endregion
    }
}