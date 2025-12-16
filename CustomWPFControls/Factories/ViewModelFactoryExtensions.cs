using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CustomWPFControls.Factories
{
    /// <summary>
    /// Extension-Methods für IServiceCollection zur Registrierung von ViewModel-Factories.
    /// </summary>
    public static class ViewModelFactoryExtensions
    {
        /// <summary>
        /// Registriert eine ViewModelFactory für TModel ? TViewModel.
        /// </summary>
        /// <typeparam name="TModel">Model-Typ.</typeparam>
        /// <typeparam name="TViewModel">ViewModel-Typ (muss ViewModelBase&lt;TModel&gt; erben).</typeparam>
        /// <param name="services">Die IServiceCollection.</param>
        /// <returns>Die IServiceCollection für Fluent-API.</returns>
        /// <example>
        /// <code>
        /// services.AddViewModelFactory&lt;Customer, CustomerItemViewModel&gt;();
        /// </code>
        /// </example>
        public static IServiceCollection AddViewModelFactory<TModel, TViewModel>(
            this IServiceCollection services)
            where TModel : class
            where TViewModel : class
        {
            services.TryAddSingleton<IViewModelFactory<TModel, TViewModel>, 
                ViewModelFactory<TModel, TViewModel>>();
            return services;
        }
    }
}
