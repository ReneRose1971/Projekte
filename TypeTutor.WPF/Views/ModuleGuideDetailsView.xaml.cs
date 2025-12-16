using System.Windows.Controls;
using System.Windows;
using TypeTutor.Logic.Core;
using Markdig;
using System.Threading.Tasks;
using System;

namespace TypeTutor.WPF
{
    public partial class ModuleGuideDetailsView : UserControl
    {
        private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        private ModuleGuideViewModel? _vm;
        private bool _webViewAvailable = true;

        public ModuleGuideDetailsView()
        {
            InitializeComponent();

            // react to DataContext changes so we can subscribe/unsubscribe safely
            this.DataContextChanged += OnDataContextChanged;
            this.Loaded += ModuleGuideDetailsView_Loaded;
        }

        private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ModuleGuideViewModel oldVm)
            {
                oldVm.PropertyChanged -= Vm_PropertyChanged;
            }

            _vm = e.NewValue as ModuleGuideViewModel;

            if (_vm is not null)
            {
                _vm.PropertyChanged += Vm_PropertyChanged;
                // fire-and-forget the update; UI thread will be used inside
                _ = UpdateViewerAsync(_vm.SelectedItem);
            }
            else
            {
                // clear view when no VM
                _ = UpdateViewerAsync(null);
            }
        }

        private async void ModuleGuideDetailsView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await EnsureWebViewInitializedAsync().ConfigureAwait(true);

                if (_vm is not null)
                {
                    await UpdateViewerAsync(_vm.SelectedItem).ConfigureAwait(true);
                }
            }
            catch (OperationCanceledException)
            {
                // mark webview as unavailable and fallback
                _webViewAvailable = false;
                ShowFallback("WebView initialisierung abgebrochen.");
            }
            catch (Exception)
            {
                // on any init failure, use fallback view but don't disturb the user with dialogs
                _webViewAvailable = false;
                ShowFallback("WebView nicht verfügbar.");
            }
        }

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModuleGuideViewModel.SelectedItem) && sender is ModuleGuideViewModel vm)
            {
                _ = UpdateViewerAsync(vm.SelectedItem);
            }
        }

        private async Task UpdateViewerAsync(ModuleGuide? mg)
        {
            // Ensure control exists
            if (WebView is null && FallbackBox is null)
                return;

            if (!_webViewAvailable)
            {
                ShowFallback(mg?.BodyMarkDown ?? string.Empty);
                return;
            }

            // Ensure WebView2 is initialized before navigating
            try
            {
                await EnsureWebViewInitializedAsync().ConfigureAwait(true);
            }
            catch
            {
                _webViewAvailable = false;
                ShowFallback(mg?.BodyMarkDown ?? string.Empty);
                return;
            }

            string doc;
            if (mg is null)
            {
                doc = "<html><body></body></html>";
            }
            else
            {
                var html = Markdig.Markdown.ToHtml(mg.BodyMarkDown ?? string.Empty, _pipeline);
                doc = $"<!doctype html><html><head><meta charset=\"utf-8\"><style>body {{ font-family: Segoe UI, Arial; padding: 12px; }}</style></head><body>{html}</body></html>";
            }

            // Marshal navigation to UI thread and guard CoreWebView2
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        if (WebView?.CoreWebView2 != null)
                        {
                            WebView.CoreWebView2.NavigateToString(doc);
                            WebView.Visibility = Visibility.Visible;
                            FallbackBox.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            // As a fallback call NavigateToString on the control after ensuring CoreWebView2 is ready
#pragma warning disable VSTHRD001 // Silence analyzer in generated edit
                            _ = WebView.EnsureCoreWebView2Async().ContinueWith(t =>
                            {
                                try
                                {
                                    if (WebView?.CoreWebView2 != null)
                                    {
                                        WebView.CoreWebView2.NavigateToString(doc);
                                        Dispatcher.Invoke(() =>
                                        {
                                            WebView.Visibility = Visibility.Visible;
                                            FallbackBox.Visibility = Visibility.Collapsed;
                                        });
                                    }
                                }
                                catch { }
                            }, TaskScheduler.FromCurrentSynchronizationContext());
#pragma warning restore VSTHRD001
                        }
                    }
                    catch { /* swallow any navigation error to avoid crashing UI */ }
                });
            }
            catch { /* ignore dispatcher errors */ }
        }

        private void ShowFallback(string text)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (FallbackBox != null)
                    {
                        FallbackBox.Text = text ?? string.Empty;
                        FallbackBox.Visibility = Visibility.Visible;
                    }
                    if (WebView != null)
                    {
                        WebView.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch { }
        }

        private async Task EnsureWebViewInitializedAsync()
        {
            if (WebView is null) throw new InvalidOperationException("WebView control is not available.");

            try
            {
                if (WebView.CoreWebView2 == null)
                {
                    await WebView.EnsureCoreWebView2Async().ConfigureAwait(true);
                }
            }
            catch
            {
                // bubble nothing; caller will handle null CoreWebView2
                throw;
            }
        }
    }
}
