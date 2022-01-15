using System;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Player;
using JKMP.Plugin.Multiplayer.Networking;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using JumpKing.Level;
using JumpKing.MiscEntities.WorldItems;
using JumpKing.Player;
using JumpKing.Player.Skins;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Game.Components
{
    public class PlayerStateTransmitter : Component
    {
        /// <summary>
        /// Gets the state transmission interval in seconds (30/sec)
        /// </summary>
        public const float TransmissionInterval = 1 / 30f;
        
        private BodyComp? body;
        private float timeSinceTransmission;
        private PlayerState lastState;
        
        private readonly P2PManager p2p;

        private LocalPlayerListener? listener;

        public PlayerStateTransmitter(P2PManager p2p)
        {
            this.p2p = p2p ?? throw new ArgumentNullException(nameof(p2p));
        }

        protected override void Init()
        {
            if (gameObject is not PlayerEntity)
                throw new NotSupportedException("The PlayerStateTransmitter component was added to a non player entity");

            body = GetComponent<BodyComp>() ?? throw new NotSupportedException("BodyComp component not found");
        }

        protected override void OnEnable()
        {
            listener = new();
        }

        protected override void OnDisable()
        {
            listener!.Dispose();
            listener = null;
        }

        protected override void OnOwnerDestroy()
        {
            OnDisable();
        }

        protected override void Update(float delta)
        {
            listener?.Update(delta);
        }

        protected override void LateUpdate(float delta)
        {
            timeSinceTransmission += delta;

            if (timeSinceTransmission >= TransmissionInterval || listener!.CurrentState != lastState)
            {
                timeSinceTransmission = 0;
                SendState();
            }

            lastState = listener!.CurrentState;
        }

        private void SendState()
        {
            if (body == null || listener == null)
                throw new InvalidOperationException("SendTranmission was called before component was initialized");

            // Only send message if the connected players mutex isn't locked (prevents potential lag spikes)
            // Note that there's still a chance it will be locked after this check and before the broadcast happens, but it should (hopefully) lower lag spikes significantly
            if (p2p.ConnectedPlayersMtx.IsLocked)
                return;

            var surfaceType = listener.CurrentSurfaceType;
            bool wearingShoes = SkinManager.IsWearingSkin(Items.Shoes);

            p2p.Broadcast(new PlayerStateChanged
            {
                Position = body.position,
                State = listener.CurrentState,
                WalkDirection = (sbyte)listener.WalkDirection,
                SurfaceType = surfaceType,
                WearingShoes = wearingShoes
            }, P2PSend.Unreliable);
        }
    }
}