using System;
using System.Collections.Generic;
using System.Reflection;
using BehaviorTree;
using JKMP.Core.Configuration.Attributes;
using JKMP.Core.Configuration.Attributes.PropertyCreators;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Components;
using JKMP.Plugin.Multiplayer.Game.Sound;
using JumpKing;
using JumpKing.Controller;
using JumpKing.PauseMenu;
using JumpKing.PauseMenu.BT;
using JumpKing.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IDrawable = JumpKing.Util.IDrawable;

namespace JKMP.Plugin.Multiplayer.Game.UI.MenuFields
{
    internal class AudioInputTestField : IBTnode, IMenuItem
    {
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;
                
                name = value;
                nameSize = font.MeasureString(name);
            }
        }
        
        private string name;
        private readonly SpriteFont font;
        private Vector2 nameSize;
        
        private readonly VoicePlayback playback;
        private readonly Sprite sliderLeft;
        private readonly Sprite sliderRight;
        private readonly Sprite sliderCursor;
        private readonly Sprite sliderLine;

        private float maxVolume;
        private float timeSinceMaxVolumeRaised;
        private bool lastPeaked;

        public AudioInputTestField(string name)
        {
            font = JKContentManager.Font.MenuFont;
            sliderLeft = JKContentManager.GUI.SliderLeft;
            sliderRight = JKContentManager.GUI.SliderRight;
            sliderCursor = JKContentManager.GUI.SliderCursor;
            sliderLine = JKContentManager.GUI.SliderLine;
            
            Name = name;
            this.name = name; // Stops the compiler from warning about non nullable field being null
            playback = new();
        }

        protected override BTresult MyRun(TickData tickData)
        {
            var padState = MenuController.instance.GetPadState();

            if (padState.confirm)
            {
                MenuController.instance.ConsumePadPresses();

                if (padState.confirm)
                {
                    if (!playback.IsCapturing)
                    {
                        bool success = playback.StartPlayback();

                        if (success)
                        {
                            return BTresult.Success;
                        }
                    }
                    else
                    {
                        playback.StopPlayback();
                        return BTresult.Success;
                    }
                }
            }
            
            return BTresult.Failure;
        }

        public void Draw(int x, int y, bool selected)
        {
            UpdateVolumeSlider(1f / PlayerValues.FPS);

            var color = lastPeaked ? Color.Red : Color.White;
            
            TextHelper.DrawString(font, name, new Vector2(x, y), playback.IsCapturing ? color : Color.LightGray, Vector2.Zero);
            
            // Draw volume indicator
            float totalWidth = 133;
            float volume = playback.CapturedVolume;
            Vector2 drawPos = new Vector2(x, y + nameSize.Y + 3);
            sliderLeft.Draw(drawPos);
            sliderRight.Draw(drawPos + new Vector2(totalWidth - sliderRight.source.Width, 0));
            
            // Draw volume line
            {
                float maxWidth = (totalWidth - sliderLeft.source.Width - sliderRight.source.Width + 1);
                
                int x2 = (int)drawPos.X + sliderLeft.source.Width - 1;
                int y2 = (int)drawPos.Y;
                int width = (int)(maxWidth * volume);
                int height = sliderLeft.source.Height;
                Game1.spriteBatch.Draw(JKContentManager.Pixel.texture, new Rectangle(x2, y2, width, height), color);
                
                // Draw max volume line
                width = 1;
                x2 += (int)(maxWidth * maxVolume);

                Game1.spriteBatch.Draw(JKContentManager.Pixel.texture, new Rectangle(x2, y2, width, height), lastPeaked ? Color.Red : Color.White);
            }
        }

        private void UpdateVolumeSlider(float delta)
        {
            if (playback.CapturedVolume > maxVolume)
            {
                maxVolume = playback.CapturedVolume;
                timeSinceMaxVolumeRaised = 0;
                lastPeaked = maxVolume >= 1f;
            }
            else
            {
                timeSinceMaxVolumeRaised += delta;
            }
            
            if (timeSinceMaxVolumeRaised > 0.8f)
            {
                maxVolume -= delta * 0.25f;
                maxVolume = MathHelper.Clamp(maxVolume, playback.CapturedVolume, 1);
            }
        }

        public Point GetSize()
        {
            var size = nameSize.ToPoint();
            size.Y += 3 + sliderLeft.source.Height;
            return size;
        }
    }

    [SettingsOptionCreator(typeof(AudioInputTestFieldCreator))]
    internal class AudioInputTestFieldAttribute : SettingsOptionAttribute
    {
    }

    internal class AudioInputTestFieldCreator : ConfigPropertyCreator<AudioInputTestFieldAttribute>
    {
        // This field doesn't have a value so we allow bool as a sort of "default" type.
        public override ICollection<Type> SupportedTypes => new[] { typeof(bool) };
        
        public override IMenuItem CreateField(object config, string fieldName, PropertyInfo propertyInfo, AudioInputTestFieldAttribute attribute, List<IDrawable> drawables)
        {
            return new AudioInputTestField(fieldName);
        }
    }
}