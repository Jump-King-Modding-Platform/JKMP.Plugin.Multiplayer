namespace Matchmaking.Client.EventData
{
    public class ServerStatus
    {
        public uint TotalPlayers { get; set; }
        public uint GroupPlayers { get; set; }

        public ServerStatus(uint totalPlayers, uint groupPlayers)
        {
            TotalPlayers = totalPlayers;
            GroupPlayers = groupPlayers;
        }
    }
}