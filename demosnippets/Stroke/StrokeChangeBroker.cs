using Microsoft.AspNet.SignalR.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;

namespace MyWhiteboard.Stroke
{
    public class StrokeChangeBroker
    {
        private static readonly Lazy<StrokeChangeBroker> StrokeChangeBrokerLazy = new Lazy<StrokeChangeBroker>(() => new StrokeChangeBroker(), LazyThreadSafetyMode.ExecutionAndPublication);
        private readonly StrokeChunkManager strokeChunkManager = new StrokeChunkManager();

        private bool alreadyStarted;
        private HubConnection connection;
        private IHubProxy hubProxy;
        private string currentSessionUri;

        private StrokeChangeBroker()
        {
        }

        public static StrokeChangeBroker Instance => StrokeChangeBrokerLazy.Value;
        public event EventHandler<StrokeDescription> StrokeCollected;
        public event EventHandler<Guid> StrokeErased;
        public event EventHandler AllStrokeErased;
        public event EventHandler ResendAllStrokesRequested;

        public async Task StartBrokerAsync(string sessionUri)
        {
            if (alreadyStarted)
            {
                return;
            }

            currentSessionUri = sessionUri;

            connection = new HubConnection(Consts.SignalRUrl);
            hubProxy = connection.CreateHubProxy("StrokeSyncHub");

            hubProxy.On<byte[]>("onStrokeCollected", compressedStrokePoints => { strokeChunkManager.ReceiveStrokePart(compressedStrokePoints, strokeDescription => StrokeCollected?.Invoke(this, strokeDescription)); });
            hubProxy.On<Guid>("onEraseStroke", strokeId => StrokeErased?.Invoke(this, strokeId));
            hubProxy.On("onEraseAllStrokes", () => AllStrokeErased?.Invoke(this, EventArgs.Empty));
            hubProxy.On("onResendAllStrokesRequested", () => ResendAllStrokesRequested?.Invoke(this, EventArgs.Empty));

            await connection.Start();

            await Task.Delay(500);
            alreadyStarted = true;

            await hubProxy.Invoke("JoinSession", currentSessionUri);
        }

        public async void StopBroker()
        {
            await hubProxy.Invoke("LeaveSession", currentSessionUri);
            connection.Stop();

            alreadyStarted = false;
        }

        public void SendStrokeCollected(Guid strokeId, InkStroke stroke)
        {
            var points = stroke.GetInkPoints().ToList();
            strokeChunkManager.SendStrokeInChunks(strokeId, points, stroke.DrawingAttributes, data => hubProxy?.Invoke(nameof(SendStrokeCollected), currentSessionUri, data));
        }

        public void SendEraseAllStrokes()
        {
            hubProxy?.Invoke(nameof(SendEraseAllStrokes), currentSessionUri);
        }

        public void SendEraseStroke(Guid strokeId)
        {
            hubProxy?.Invoke(nameof(SendEraseStroke), currentSessionUri, strokeId);
        }

        public void RequestResendAllStrokes()
        {
            hubProxy?.Invoke("ResendAllStrokes", currentSessionUri);
        }
    }
}