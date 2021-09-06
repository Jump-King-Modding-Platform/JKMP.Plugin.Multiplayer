using System;
using System.Collections.Generic;

namespace Matchmaking.Client
{
    public class Events
    {
        public delegate void NearbyClientsReceivedHandler(ICollection<ulong> steamIds);

        public event NearbyClientsReceivedHandler? NearbyClientsReceived;

        internal void OnNearbyClientsReceived(ICollection<ulong> steamIds)
        {
            if (steamIds == null) throw new ArgumentNullException(nameof(steamIds));
            NearbyClientsReceived?.Invoke(steamIds);
        }
    }
}