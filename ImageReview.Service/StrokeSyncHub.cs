using System;
using Microsoft.AspNet.SignalR;

namespace ImageReview.Service
{
    public class StrokeSyncHub : Hub
    {
        public void SendStrokeCollected(object strokeDefinition)
        {
            Clients.All.onStrokeCollected(strokeDefinition);
        }

        public void SendEraseStroke(Guid strokeId)
        {
            Clients.All.onEraseStroke(strokeId);
        }

        public void SendEraseAllStrokes()
        {
            Clients.All.onEraseAllStrokes();
        }

        public void UpdateMachinePresence(string machineName, bool isPresent)
        {
            Clients.All.onUpdateMachinePresence(machineName, isPresent);
        }

        public void MachineIsOffline(string machineName)
        {
            Clients.All.onMachineOffline(machineName);
        }

        public void BackgroundImageChanged(string uri)
        {
            Clients.All.onBackgroundImageChanged(uri);
        }
    }
}