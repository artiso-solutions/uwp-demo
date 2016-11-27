using MyWhiteboard.ImageHandling;
using MyWhiteboard.Stroke;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyWhiteboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private readonly string machineName;
        private readonly DispatcherTimer presenceTimer;
        private bool currentPublishedPresence;
        private DateTime lastPresentStateTimestamp = DateTime.UtcNow;
        private DeviceWatcher presenceWatcher;
        private StrokeSynchronization strokeSynchronization;

        public MainPage()
        {
            InitializeComponent();

            StrokeChangeBroker.Instance.PresenceChanged += MachinePresenceChanged;
            StrokeChangeBroker.Instance.MachineOffline += MachineOffline;
            StrokeChangeBroker.Instance.BackgroundImageChanged += BackgroundImageChanged;

            PresenceInfos = new ObservableCollection<PresenceInfo>();

            var hostNames = NetworkInformation.GetHostNames();
            machineName = hostNames.FirstOrDefault(name => name.Type == HostNameType.DomainName)?.DisplayName ?? "???";

            presenceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            presenceTimer.Tick += CheckForPresence;
        }

        public ObservableCollection<PresenceInfo> PresenceInfos { get; }

        private async void OnPresenceSensorAdded(DeviceWatcher sender, DeviceInformation device)
        {
            var proximitySensor = ProximitySensor.FromId(device.Id);
            Debug.WriteLine("Presence sensor connected");

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { presenceTimer.Start(); });

            proximitySensor.ReadingChanged += (sensor, args) =>
            {
                var isPresent = args.Reading.IsDetected;
                if (isPresent)
                {
                    lastPresentStateTimestamp = DateTime.UtcNow;
                    if (!currentPublishedPresence)
                    {
                        CheckForPresence(null, null);
                    }
                }
            };
        }

        private void CheckForPresence(object sender, object o)
        {
            var isPresent = lastPresentStateTimestamp >= DateTime.UtcNow.AddSeconds(-5);
            if (currentPublishedPresence != isPresent)
            {
                currentPublishedPresence = isPresent;
                StrokeChangeBroker.Instance.UpdateMachineState(machineName, currentPublishedPresence);
            }
        }

        private async void OnLoadedMainPage(object sender, RoutedEventArgs e)
        {
            InkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Touch;

            strokeSynchronization = new StrokeSynchronization(InkCanvas, InkToolbar, StrokeChangeBroker.Instance);

            await StrokeChangeBroker.Instance.StartBrokerAsync();
            
            StrokeChangeBroker.Instance.UpdateMachineState(machineName, true);
            StrokeChangeBroker.Instance.RequestResendAllStrokes();

            Task.Run(() =>
            {
                presenceWatcher = DeviceInformation.CreateWatcher(ProximitySensor.GetDeviceSelector());
                presenceWatcher.Added += OnPresenceSensorAdded;
                presenceWatcher.Start();
            });

            var backImageSource = await ImageAccess.LoadCurrentBackgroundImageAsync();
            BackImage.Source = backImageSource;
        }

        private async void MachinePresenceChanged(object sender, PresenceInfo presenceInfo)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var existingInfo = PresenceInfos.FirstOrDefault(pi => pi.MachineName == presenceInfo.MachineName);
                if (existingInfo == null)
                {
                    PresenceInfos.Add(presenceInfo);
                }
                else
                {
                    existingInfo.IsPresent = presenceInfo.IsPresent;
                }
            });
        }

        private async void MachineOffline(object sender, string machineName)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var existingInfo = PresenceInfos.FirstOrDefault(pi => pi.MachineName == machineName);
                if (existingInfo != null)
                {
                    PresenceInfos.Remove(existingInfo);
                }
            });
        }

        private async void BackgroundImageChanged(object sender, string imageUri)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (imageUri == null)
                {
                    BackImage.Source = null;
                    return;
                }

                var backImageSource = ImageAccess.LoadImage(imageUri);
                BackImage.Source = backImageSource;
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            systemNavigationManager.BackRequested += SystemNavigationManagerOnBackRequested;
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
            presenceTimer.Stop();

            StrokeChangeBroker.Instance.UpdateMachineState(machineName, false);
        }
    }
}