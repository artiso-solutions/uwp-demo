using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace MyWhiteboard.Service
{
    public class StrokeSyncHub : Hub
    {
        public async Task JoinSession(string sessionUri)
        {
            await Groups.Add(Context.ConnectionId, sessionUri);
        }

        public async Task LeaveSession(string sessionUri)
        {
            await Groups.Remove(Context.ConnectionId, sessionUri);
        }

        public void SendStrokeCollected(string sessionUri, object strokeDefinition)
        {
            Clients.Group(sessionUri).onStrokeCollected(strokeDefinition);
        }

        public void SendEraseStroke(string sessionUri, Guid strokeId)
        {
            Clients.Group(sessionUri).onEraseStroke(strokeId);
        }

        public void SendEraseAllStrokes(string sessionUri)
        {
            Clients.Group(sessionUri).onEraseAllStrokes();
        }

        public void ResendAllStrokes(string sessionUri)
        {
            Clients.Group(sessionUri).onResendAllStrokesRequested();
        }
    }
}