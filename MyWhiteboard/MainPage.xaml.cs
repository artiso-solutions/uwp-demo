using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MyWhiteboard
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        public SelectImageViewModel ViewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            ViewModel = new SelectImageViewModel();
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var backgroundImageDescription = (BackgroundImageDescription)e.ClickedItem;

            Frame.Navigate(typeof(DrawingCanvas), backgroundImageDescription.ImageUri);
        }
    }
}