<TextBlock Text="Hello UWP!" />

---> Update NuGetPackage UniversalWindowsPlatform

-------------------------------------
--- MainPage.xaml

<Page.Resources>
	<Style TargetType="GridViewItem">
		<Setter Property="VerticalContentAlignment" Value="Stretch" />
		<Setter Property="HorizontalContentAlignment" Value="Stretch" />
	</Style>
</Page.Resources>

---

<Grid.RowDefinitions>
	<RowDefinition Height="Auto" />
	<RowDefinition Height="*" />
</Grid.RowDefinitions>

---

<GridView Grid.Row="1">
	<Rectangle x:Name="Blue1" Fill="Blue" Width="300"  Height="200" />
	<Rectangle Fill="Red" />
	<Rectangle Fill="Blue" />
	<Rectangle Fill="Red" />
	<Rectangle Fill="Blue" />
	<Rectangle Fill="Red" />
</GridView>

---

<VisualStateManager.VisualStateGroups>
	<VisualStateGroup x:Name="WindowStates">
		<VisualState x:Name="WideState">
			<VisualState.StateTriggers>
				<AdaptiveTrigger MinWindowWidth="800" />
			</VisualState.StateTriggers>
			<VisualState.Setters>
				<Setter Target="Blue1.Width" Value="500" />
				<Setter Target="Blue1.Height" Value="300" />
			</VisualState.Setters>
		</VisualState>
		<VisualState x:Name="NarrowState">
			<VisualState.StateTriggers>
				<AdaptiveTrigger MinWindowWidth="0" />
			</VisualState.StateTriggers>
			<VisualState.Setters>
				<Setter Target="Blue1.Width" Value="300" />
				<Setter Target="Blue1.Height" Value="200" />
			</VisualState.Setters>
		</VisualState>
	</VisualStateGroup>
</VisualStateManager.VisualStateGroups>

--- show in Blend

----------------------------------------------------
--- Consts.cs

public class Consts
{
	public const string ApiControllerBaseUrl = "<yoururl>/api/review/";

	public const string SignalRUrl = "<yoururl>/signalr";
}

---> Install Nuget Package: Microsoft.AspNet.WebApi.Client

--- BackgroundImageDescription
namespace MyWhiteboard
{
    public class BackgroundImageDescription
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public string ThumbnailUri { get; set; }

        public string ImageUri { get; set; }
    }
}


--- SelectImageViewModel
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MyWhiteboard
{
    public class SelectImageViewModel : INotifyPropertyChanged
    {
        private bool isLoading;

        public SelectImageViewModel()
        {
            Images = new ObservableCollection<BackgroundImageDescription>();

            LoadImages();
        }

        public ObservableCollection<BackgroundImageDescription> Images { get; }

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (value == isLoading)
                {
                    return;
                }
                isLoading = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void LoadImages()
        {
            IsLoading = true;
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(new Uri($"{Consts.ApiControllerBaseUrl}GetImageDescriptions"));
            var backgroundImages = await response.Content.ReadAsAsync<List<BackgroundImageDescription>>();
            foreach (var backgroundImage in backgroundImages)
            {
                Images.Add(backgroundImage);
                await Task.Delay(500);
            }

            IsLoading = false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

--- MainPage.xaml.cs (replace content)
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

        private void GridViewOnItemClick(object sender, ItemClickEventArgs e)
        {
            var backgroundImageDescription = (BackgroundImageDescription)e.ClickedItem;
        }
    }
}



--- MainPage.xaml
<Page.Resources>
	<DataTemplate x:Key="BackgroundImageTemplateWide"
				  x:DataType="local:BackgroundImageDescription">
		<Border x:Name="ItemBorder"
				Width="300"
				Height="200"
				Margin="8"
				Background="{ThemeResource ButtonBackgroundThemeBrush}">
			<Grid>
				<Grid.ColumnDefinitions>
					<ColumnDefinition Width="1*" />
					<ColumnDefinition Width="2*" />
				</Grid.ColumnDefinitions>

				<TextBlock Grid.Column="0"
						   Grid.ColumnSpan="2"
						   Margin="8"
						   VerticalAlignment="Bottom"
						   FontWeight="Bold"
						   Foreground="{ThemeResource ButtonForegroundThemeBrush}"
						   Text="{x:Bind Description}" />
				<Image Grid.Column="1"
					   Margin="8"
					   Source="{x:Bind ThumbnailUri}"
					   Stretch="Uniform" />
			</Grid>
		</Border>
	</DataTemplate>

	<DataTemplate x:Key="BackgroundImageTemplateNarrow"
				  x:DataType="local:BackgroundImageDescription">
		<Border x:Name="ItemBorder"
				Width="256"
				Height="80"
				Margin="4"
				Background="{ThemeResource ButtonBackgroundThemeBrush}">
			<Grid>
				<Grid.ColumnDefinitions>
					<ColumnDefinition Width="1*" />
					<ColumnDefinition Width="2*" />
				</Grid.ColumnDefinitions>

				<TextBlock Grid.Column="0"
						   Grid.ColumnSpan="2"
						   Margin="4"
						   VerticalAlignment="Bottom"
						   FontWeight="Bold"
						   Foreground="{ThemeResource ButtonForegroundThemeBrush}"
						   Text="{x:Bind Description}" />
				<Image Grid.Column="1"
					   Margin="4"
					   Source="{x:Bind ThumbnailUri}"
					   Stretch="Uniform" />
			</Grid>
		</Border>
	</DataTemplate>
</Page.Resources>

 <Grid x:Name="LayoutRoot"
	  Margin="24 8"
	  Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
	<Grid.RowDefinitions>
		<RowDefinition Height="Auto" />
		<RowDefinition Height="Auto" />
		<RowDefinition Height="*" />
	</Grid.RowDefinitions>

	<TextBlock Grid.Row="0"
			   Style="{ThemeResource TitleTextBlockStyle}"
			   Text="My Whiteboard" />
	<TextBlock Grid.Row="1"
			   Style="{ThemeResource SubtitleTextBlockStyle}"
			   Text="Select Image" />

	<ProgressRing Grid.Row="2"
				  Width="50"
				  Height="50"
				  HorizontalAlignment="Center"
				  VerticalAlignment="Center"
				  IsActive="{x:Bind ViewModel.IsLoading, Mode=OneWay}" />

	<GridView x:Name="RoomsGridView"
			  Grid.Row="2"
			  IsItemClickEnabled="True"
			  ItemClick="GridViewOnItemClick"
			  ItemTemplate="{StaticResource BackgroundImageTemplateWide}"
			  ItemsSource="{x:Bind ViewModel.Images}"
			  SelectionMode="None" />

	<VisualStateManager.VisualStateGroups>
		<VisualStateGroup x:Name="WindowStates">
			<VisualState x:Name="WideState">
				<VisualState.StateTriggers>
					<AdaptiveTrigger MinWindowWidth="800" />
				</VisualState.StateTriggers>
				<VisualState.Setters>
					<Setter Target="LayoutRoot.Margin" Value="24 8" />
					<Setter Target="RoomsGridView.ItemTemplate" Value="{StaticResource BackgroundImageTemplateWide}" />
				</VisualState.Setters>
			</VisualState>
			<VisualState x:Name="NarrowState">
				<VisualState.StateTriggers>
					<AdaptiveTrigger MinWindowWidth="0" />
				</VisualState.StateTriggers>
				<VisualState.Setters>
					<Setter Target="LayoutRoot.Margin" Value="12 8" />
					<Setter Target="RoomsGridView.ItemTemplate" Value="{StaticResource BackgroundImageTemplateNarrow}" />
				</VisualState.Setters>
			</VisualState>
		</VisualStateGroup>
	</VisualStateManager.VisualStateGroups>
</Grid>

---------------------------------------------------------------------
--- DrawingCanvas.xaml
<Page x:Class="MyWhiteboard.DrawingCanvas"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
      xmlns:local="using:MyWhiteboard"
      xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
      d:DesignHeight="300"
      d:DesignWidth="400"
      Loaded="OnLoadedMainPage"
      mc:Ignorable="d">

    <Grid Background="#FFE6E6E6">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <Viewbox Grid.Row="1">
            <Grid Width="1920"
                  Height="1200"
                  Background="#FFFFFFFF">
                <Image x:Name="BackImage" Stretch="Uniform" />
            </Grid>
        </Viewbox>
    </Grid>
</Page>

--- DrawingCanvas.xaml.cs
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MyWhiteboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DrawingCanvas
    {
        private string currentSessionUri;

        public DrawingCanvas()
        {
            InitializeComponent();
        }

        private void OnLoadedMainPage(object sender, RoutedEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            systemNavigationManager.BackRequested += SystemNavigationManagerOnBackRequested;

            currentSessionUri = (string)e.Parameter;
            var backImageSource = LoadImage(currentSessionUri);
            BackImage.Source = backImageSource;
        }

        private void SystemNavigationManagerOnBackRequested(object sender, BackRequestedEventArgs backRequestedEventArgs)
        {
            if (Frame.CanGoBack)
            {
                backRequestedEventArgs.Handled = true;
                Frame.GoBack();
            }
        }

        public static BitmapImage LoadImage(string url)
        {
            return new BitmapImage(new Uri(url, UriKind.Absolute));
        }
    }
}

--- MainPage.xaml.cs
private void GridViewOnItemClick(object sender, ItemClickEventArgs e)
{
	var backgroundImageDescription = (BackgroundImageDescription)e.ClickedItem;
	Frame.Navigate(typeof(DrawingCanvas), backgroundImageDescription.ImageUri);
}

---------------------------------------------------
--- DrawingCanvas.xaml
<InkCanvas x:Name="InkCanvas" />

--- DrawingCanvas.xaml.cs
private void OnLoadedMainPage(object sender, RoutedEventArgs e)
{
	InkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Touch;
}

--- DrawingCanvas.xaml
<InkToolbar x:Name="InkToolbar"
                    Grid.Row="0"
                    TargetInkCanvas="{Binding ElementName=InkCanvas}" />

private void OnLoadedMainPage(object sender, RoutedEventArgs e)
{
	InkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Touch;
}


---------------------------------------------------
--- Install NuGet Package: Microsoft.AspNet.SignalR.Client
--- Install NuGet Package: protobuf-portable-net

--- copy files from Folder Stroke to new folder Stroke and include in project

--- DrawingCanvas.xaml.cs
private StrokeSynchronization strokeSynchronization;

---
private void OnLoadedMainPage(object sender, RoutedEventArgs e)
{
	...
    strokeSynchronization = new StrokeSynchronization(InkCanvas, InkToolbar, StrokeChangeBroker.Instance);
}

---
protected override async void OnNavigatedTo(NavigationEventArgs e)
{
	...
	
	await StrokeChangeBroker.Instance.StartBrokerAsync(currentSessionUri);
	StrokeChangeBroker.Instance.RequestResendAllStrokes();
}

---
protected override void OnNavigatedFrom(NavigationEventArgs e)
{
	strokeSynchronization.StopSynchronization();
}