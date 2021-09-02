using System;
using EntityComponent;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public abstract class BaseManagerEntity : Entity
    {
        private bool isFirstUpdate = true;
        
        protected override void Update(float delta)
        {
            if (isFirstUpdate)
            {
                isFirstUpdate = false;
                OnFirstUpdate();
            }
        }

        protected virtual void OnFirstUpdate()
        {
        }
    }
}