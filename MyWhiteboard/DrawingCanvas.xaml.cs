using MyWhiteboard.Stroke;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyWhiteboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DrawingCanvas
    {
        private StrokeSynchronization strokeSynchronization;
        private string currentSessionUri;

        public DrawingCanvas()
        {
            InitializeComponent();
        }

        private void OnLoadedMainPage(object sender, RoutedEventArgs e)
        {
            InkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Touch;

            strokeSynchronization = new StrokeSynchronization(InkCanvas, InkToolbar, StrokeChangeBroker.Instance);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            systemNavigationManager.BackRequested += SystemNavigationManagerOnBackRequested;

            currentSessionUri = (string)e.Parameter;
            var backImageSource = ImageAccess.LoadImage(currentSessionUri);
            BackImage.Source = backImageSource;

            await StrokeChangeBroker.Instance.StartBrokerAsync(currentSessionUri);
            StrokeChangeBroker.Instance.RequestResendAllStrokes();
        }

        private void SystemNavigationManagerOnBackRequested(object sender, BackRequestedEventArgs backRequestedEventArgs)
        {
            if (Frame.CanGoBack)
            {
                backRequestedEventArgs.Handled = true;
                Frame.GoBack();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            strokeSynchronization.StopSynchronization();
        }
    }
}