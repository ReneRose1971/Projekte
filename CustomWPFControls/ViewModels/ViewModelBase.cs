using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;

namespace CustomWPFControls.ViewModels
{
    /// <summary>
    /// Basisklasse für alle ViewModels mit automatischem PropertyChanged.
    /// Wraps ein Domain-Model und bietet Referenz-basierte Gleichheit.
    /// </summary>
    /// <typeparam name="TModel">Der Typ des gewrappten Models.</typeparam>
    /// <remarks>
    /// <para>
    /// <b>PropertyChanged:</b> Durch <c>[AddINotifyPropertyChangedInterface]</c> wird automatisch
    /// INotifyPropertyChanged implementiert (via Fody.PropertyChanged).
    /// </para>
    /// <para>
    /// <b>Equals-Semantik:</b> ViewModels sind gleich, wenn sie die gleiche Model-Referenz wrappen.
    /// </para>
    /// <para>
    /// <b>GetHashCode:</b> Basiert auf Model-Referenz (ReferenceEqualityComparer).
    /// </para>
    /// </remarks>
    [AddINotifyPropertyChangedInterface]
    public abstract class ViewModelBase<TModel> : IViewModelWrapper<TModel>, INotifyPropertyChanged
        where TModel : class
    {
        /// <summary>
        /// Das gewrappte Domain-Model.
        /// </summary>
        public TModel Model { get; }

        /// <summary>
        /// Erstellt ein ViewModel für das gegebene Model.
        /// </summary>
        /// <param name="model">Das zu wrappende Model.</param>
        /// <exception cref="ArgumentNullException">Wenn <paramref name="model"/> null ist.</exception>
        protected ViewModelBase(TModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// GetHashCode basiert auf Model-Referenz (RuntimeHelpers.GetHashCode).
        /// </summary>
        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Model);
        }

        /// <summary>
        /// Equals prüft auf gleiche Model-Referenz (ReferenceEquals).
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not ViewModelBase<TModel> other)
                return false;

            return ReferenceEquals(Model, other.Model);
        }

        /// <summary>
        /// Delegiert ToString an das Model (für Debugging).
        /// </summary>
        public override string ToString() => Model.ToString() ?? GetType().Name;

        // PropertyChanged event for INotifyPropertyChanged (Fody will weave implementation)
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
