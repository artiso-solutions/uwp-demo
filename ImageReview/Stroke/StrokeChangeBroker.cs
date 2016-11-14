using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;
using Microsoft.AspNet.SignalR.Client;

namespace ImageReview.Stroke
{
    public class StrokeChangeBroker
    {
        private static readonly Lazy<StrokeChangeBroker> StrokeChangeBrokerLazy = new Lazy<StrokeChangeBroker>(() => new StrokeChangeBroker(), LazyThreadSafetyMode.ExecutionAndPublication);
        private readonly StrokeChunkManager strokeChunkManager = new StrokeChunkManager();

        private bool alreadyStarted;
        private HubConnection connection;
        private IHubProxy hubProxy;

        private StrokeChangeBroker()
        {
        }

        public static StrokeChangeBroker Instance => StrokeChangeBrokerLazy.Value;
        public event EventHandler<StrokeDescription> StrokeCollected;
        public event EventHandler<Guid> StrokeErased;
        public event EventHandler AllStrokeErased;
        public event EventHandler<PresenceInfo> PresenceChanged;
        public event EventHandler<string> MachineOffline;
        public event EventHandler<string> BackgroundImageChanged;

        public async Task StartBrokerAsync()
        {
            if (alreadyStarted)
            {
                return;
            }

            connection = new HubConnection(Consts.SignalRUrl);
            hubProxy = connection.CreateHubProxy("StrokeSyncHub");

            hubProxy.On<byte[]>("onStrokeCollected", compressedStrokePoints => { strokeChunkManager.ReceiveStrokePart(compressedStrokePoints, strokeDescription => StrokeCollected?.Invoke(this, strokeDescription)); });
            hubProxy.On<Guid>("onEraseStroke", strokeId => StrokeErased?.Invoke(this, strokeId));
            hubProxy.On("onEraseAllStrokes", () => AllStrokeErased?.Invoke(this, EventArgs.Empty));

            hubProxy.On<string, bool>("onUpdateMachinePresence", (machineName, isPresent) => PresenceChanged?.Invoke(this, new PresenceInfo
            {
                MachineName = machineName, IsPresent = isPresent
            }));
            hubProxy.On<string>("onMachineOffline", machineName => MachineOffline?.Invoke(this, machineName));
            hubProxy.On<string>("onBackgroundImageChanged", uri => BackgroundImageChanged?.Invoke(this, uri));

            await connection.Start();

            await Task.Delay(500);
            alreadyStarted = true;
        }

        public void StopBroker()
        {
            connection.Stop();
        }

        public void SendStrokeCollected(Guid strokeId, InkStroke stroke)
        {
            var points = stroke.GetInkPoints().ToList();
            strokeChunkManager.SendStrokeInChunks(strokeId, points, stroke.DrawingAttributes, data => hubProxy?.Invoke(nameof(SendStrokeCollected), data));
        }

        public void SendEraseAllStrokes()
        {
            hubProxy?.Invoke(nameof(SendEraseAllStrokes));
        }

        public void SendEraseStroke(Guid strokeId)
        {
            hubProxy?.Invoke(nameof(SendEraseStroke), strokeId);
        }

        public void UpdateMachineState(string machineName, bool isPresent)
        {
            hubProxy?.Invoke("UpdateMachinePresence", machineName, isPresent);
        }

        public void SendBackgroundImageChanged(string imageUri)
        {
            hubProxy?.Invoke("BackgroundImageChanged", imageUri);
        }

        public void SendMachineIsOffline(string machineName)
        {
            hubProxy?.Invoke("MachineIsOffline", machineName);
        }
    }
}