using System;
using System.Collections.Generic;
using System.Linq;
using JKMP.Core.Logging;
using JumpKing;
using JumpKing.XnaWrappers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JKMP.Plugin.Multiplayer.Game.Player.Animations
{
    public class SpriteFrame
    {
        public Sprite Sprite { get; set; }
        public float MinDuration { get; set; }
        public float MaxDuration { get; set; }

        private static readonly Random Random = new();

        public SpriteFrame(Sprite sprite, float minDuration, float? maxDuration = null)
        {
            Sprite = sprite;
            MinDuration = minDuration;
            MaxDuration = maxDuration ?? minDuration;
        }

        public float GetRandomDuration()
        {
            if (MinDuration.Equals(MaxDuration))
                return MinDuration;

            return MinDuration + (float)Random.NextDouble() * (MaxDuration - MinDuration);
        }

        public void Update(float dt)
        {
            if (Sprite is SpriteAnimation spriteAnimation)
            {
                spriteAnimation.Update(dt);
            }
        }
    }

    public class SpriteAnimation : Sprite
    {
        private SpriteFrame[] frames;
        private float currentDuration;
        
        private int currentFrameIndex;
        private float currentFrameDuration;
        
        private SpriteFrame CurrentFrame => frames[currentFrameIndex];
        private Sprite CurrentSprite => CurrentFrame.Sprite;
        
        public SpriteAnimation(IEnumerable<SpriteFrame> frames)
        {
            this.frames = frames.ToArray();

            if (this.frames.Length == 0)
                throw new ArgumentException("Animation must have at least one frame", nameof(frames));

            currentFrameDuration = CurrentFrame.GetRandomDuration();
        }

        public SpriteAnimation(params SpriteFrame[] frames) : this(frames.AsEnumerable())
        {
        }

        public SpriteAnimation(ICollection<SpriteFrame> frames) : this(frames.AsEnumerable())
        {
        }

        public void Reset()
        {
            currentFrameIndex = 0;
            currentFrameDuration = CurrentFrame.GetRandomDuration();
            currentDuration = 0;

            foreach (var frame in frames)
            {
                if (frame.Sprite is SpriteAnimation spriteAnim)
                    spriteAnim.Reset();
            }
        }

        public void Update(float dt)
        {
            currentDuration += dt;

            if (currentDuration >= currentFrameDuration)
            {
                currentFrameIndex += 1;

                if (currentFrameIndex >= frames.Length)
                    currentFrameIndex = 0;
                
                currentFrameDuration = CurrentFrame.GetRandomDuration();
                currentDuration = 0;
            }

            CurrentFrame.Update(dt);
        }

        public override void Draw(Vector2 pos, SpriteEffects effect = SpriteEffects.None)
        {
            CurrentSprite.Draw(pos, effect);
        }

        public override void Draw(Rectangle dst, SpriteEffects effect = SpriteEffects.None)
        {
            CurrentSprite.Draw(dst, effect);
        }

        public override void Draw(float x, float y, SpriteEffects effect = SpriteEffects.None)
        {
            CurrentSprite.Draw(x, y, effect);
        }

        public override void Draw(Point pos, SpriteEffects effect = SpriteEffects.None)
        {
            CurrentSprite.Draw(pos, effect);
        }
    }
}