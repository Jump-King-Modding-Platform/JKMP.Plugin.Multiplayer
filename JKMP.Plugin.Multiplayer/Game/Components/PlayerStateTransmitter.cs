using System;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Player;
using JKMP.Plugin.Multiplayer.Memory;
using JKMP.Plugin.Multiplayer.Networking;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using JumpKing.Level;
using JumpKing.MiscEntities.WorldItems;
using JumpKing.Player;
using JumpKing.Player.Skins;
using Steamworks;
using Steamworks.Data;

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
        private PlayerStateChanged? lastState;
        
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

            if (timeSinceTransmission >= TransmissionInterval || listener!.CurrentState != lastState?.State)
            {
                timeSinceTransmission = 0;
                SendState();
            }
        }

        private void SendState()
        {
            if (body == null || listener == null)
                throw new InvalidOperationException("SendState was called before component was initialized");

            var surfaceType = listener.CurrentSurfaceType;
            bool wearingShoes = SkinManager.IsWearingSkin(Items.Shoes);

            var playerState = Pool.Get<PlayerStateChanged>();
            playerState.Position = body.position;
            playerState.State = listener.CurrentState;
            playerState.WalkDirection = (sbyte)listener.WalkDirection;
            playerState.SurfaceType = surfaceType;
            playerState.WearingShoes = wearingShoes;
            playerState.CalculateDelta(lastState);

            p2p.Broadcast(playerState, SendType.Unreliable);

            if (lastState != null)
                Pool.Release(lastState);

            lastState = playerState;
        }
    }
}