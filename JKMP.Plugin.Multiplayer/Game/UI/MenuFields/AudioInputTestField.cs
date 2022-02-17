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

        public AudioInputTestField(string name)
        {
            font = JKContentManager.Font.MenuFont;
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
                        LogManager.TempLogger.Debug("Start playback of {deviceName} success: {success}", VoiceManager.SelectedDeviceName, success);

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
            TextHelper.DrawString(font, name, new Vector2(x, y), playback.IsCapturing ? Color.White : Color.LightGray, Vector2.Zero);
        }

        public Point GetSize() => nameSize.ToPoint();
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