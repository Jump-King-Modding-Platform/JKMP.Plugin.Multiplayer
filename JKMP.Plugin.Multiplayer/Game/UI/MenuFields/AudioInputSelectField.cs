using System;
using System.Collections.Generic;
using System.Reflection;
using BehaviorTree;
using JKMP.Core.Configuration.Attributes;
using JKMP.Core.Configuration.Attributes.PropertyCreators;
using JKMP.Plugin.Multiplayer.Native.Audio;
using JumpKing.PauseMenu;
using Microsoft.Xna.Framework;
using IDrawable = JumpKing.Util.IDrawable;

namespace JKMP.Plugin.Multiplayer.Game.UI.MenuFields
{
    internal class AudioInputSelectField : IBTnode, IMenuItem
    {
        /// <summary>
        /// Gets the selected device.
        /// </summary>
        public DeviceInformation? SelectedDeviceName { get; private set; }

        public Action<DeviceInformation>? OnDeviceSelected { get; set; }

        private readonly AudioCaptureContext captureContext;
        private readonly OpusContext opusContext;

        public AudioInputSelectField()
        {
            captureContext = new();
            opusContext = new OpusContext(48000);
        }

        public override void OnDispose()
        {
            captureContext.Dispose();
            opusContext.Dispose();
        }
        
        protected override BTresult MyRun(TickData tickData)
        {
            return BTresult.Failure;
        }

        public void Draw(int x, int y, bool selected)
        {
            
        }

        public Point GetSize()
        {
            return Point.Zero;
        }
    }

    [SettingsOptionCreator(typeof(AudioInputSelectFieldCreator))]
    internal class AudioInputSelectFieldAttribute : SettingsOptionAttribute
    {
    }

    internal class AudioInputSelectFieldCreator : ConfigPropertyCreator<AudioInputSelectFieldAttribute>
    {
        public override ICollection<Type> SupportedTypes => new[] { typeof(DeviceInformation) };
        
        public override IMenuItem CreateField(object config, string fieldName, PropertyInfo propertyInfo, AudioInputSelectFieldAttribute attribute, List<IDrawable> drawables)
        {
            var field = new AudioInputSelectField();

            field.OnDeviceSelected += (val) => ValueChanged?.Invoke(val);
            
            return field;
        }
    }
}