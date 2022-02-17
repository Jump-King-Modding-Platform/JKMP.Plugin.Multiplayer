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

            if (padState.confirm || padState.cancel || padState.pause)
            {
                MenuController.instance.ConsumePadPresses();

                if (padState.confirm && last_result != BTresult.Running)
                {
                    bool success = playback.StartPlayback();
                    LogManager.TempLogger.Debug("Start playback of {deviceName} success: {success}", VoiceManager.SelectedDeviceName, success);
                    
                    return BTresult.Running;
                }

                if (last_result == BTresult.Running)
                {
                    playback.StopPlayback();
                    LogManager.TempLogger.Debug("Stop playback of {deviceName}", VoiceManager.SelectedDeviceName);
                    
                    return BTresult.Failure;
                }
            }

            if (last_result == BTresult.Running)
                return BTresult.Running;
            
            return BTresult.Failure;
        }

        public void Draw(int x, int y, bool selected)
        {
            TextHelper.DrawString(font, name, new Vector2(x, y), last_result == BTresult.Running ? Color.White : Color.LightGray, Vector2.Zero);
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