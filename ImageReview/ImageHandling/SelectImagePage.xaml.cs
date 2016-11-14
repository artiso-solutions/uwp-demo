using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ImageReview.ImageHandling
{
    public sealed partial class SelectImagePage
    {
        public SelectImagePage()
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

        private async void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var backgroundImageDescription = (BackgroundImageDescription)e.ClickedItem;
            await ViewModel.SaveCurrentBackgroundImageAsync(backgroundImageDescription);

            Frame.Navigate(typeof(MainPage), backgroundImageDescription.Id);
        }
    }
}