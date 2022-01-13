using System;

namespace Matchmaking.Client
{
    public class MatchmakingConnectException : Exception
    {
        public MatchmakingConnectException(string errorMessage) : base(errorMessage)
        {
        }
    }
}