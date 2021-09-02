using System;

namespace Matchmaking.Client
{
    public class HostnameNotFoundException : Exception
    {
        public HostnameNotFoundException(string hostname) : base($"Could not resolve the ip address for the specified hostname '{hostname}'.")
        {
        }
    }
}