using JKMP.Plugin.Multiplayer.Matchmaking;
using JKMP.Plugin.Multiplayer.Networking;
using JumpKing.PauseMenu;
using Matchmaking.Client.EventData;
using Myra.Graphics2D.UI;

namespace JKMP.Plugin.Multiplayer.Game.UI.Widgets
{
    public class StatusPanel : ResourceWidget<StatusPanel>
    {
        public string TotalPlayers
        {
            get => totalPlayers.Text;
            set => totalPlayers.Text = value;
        }

        public string GroupPlayers
        {
            get => groupPlayers.Text;
            set => groupPlayers.Text = value;
        }

        public bool Connected
        {
            get => connected;
            set
            {
                if (value == connected)
                    return;
                
                connected = value;
                
                connectedContainer.Visible = connected;
                disconnectedContainer.Visible = !connected;
            }
        }

        private readonly Label totalPlayers;
        private readonly Label groupPlayers;
        private readonly Widget connectedContainer;
        private readonly Widget disconnectedContainer;
        private bool connected;

        public StatusPanel() : base("UI/Status/StatusPanel.xmmp")
        {
            totalPlayers = EnsureWidgetById<Label>("TotalPlayers");
            groupPlayers = EnsureWidgetById<Label>("GroupPlayers");
            connectedContainer = EnsureWidgetById<Widget>("ConnectedContainer");
            disconnectedContainer = EnsureWidgetById<Widget>("DisconnectedContainer");

            connectedContainer.Visible = false;
            
            AcceptsKeyboardFocus = false;

            MatchmakingManager.Instance.Events.ServerStatusUpdateReceived += OnServerStatusReceived;
        }

        private void OnServerStatusReceived(ServerStatus status)
        {
            TotalPlayers = status.TotalPlayers.ToString();
            GroupPlayers = status.GroupPlayers.ToString();
        }

        public void Update(float delta)
        {
            Visible = PauseManager.instance.IsPaused;
            Connected = MatchmakingManager.Instance.IsConnected;
        }
    }
}