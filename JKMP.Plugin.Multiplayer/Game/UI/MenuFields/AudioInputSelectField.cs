using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BehaviorTree;
using HarmonyLib;
using JKMP.Core.Configuration.Attributes;
using JKMP.Core.Configuration.Attributes.PropertyCreators;
using JKMP.Plugin.Multiplayer.Native.Audio;
using JumpKing;
using JumpKing.Controller;
using JumpKing.PauseMenu;
using JumpKing.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IDrawable = JumpKing.Util.IDrawable;

namespace JKMP.Plugin.Multiplayer.Game.UI.MenuFields
{
    internal class AudioInputSelectField : IBTnode, IMenuItem
    {
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;
                
                name = value;
                nameSize = nameFont.MeasureString(value);
            }
        }

        /// <summary>
        /// Gets the selected device.
        /// </summary>
        public DeviceInformation? SelectedDevice => devices.Count > 0 ? devices[selectedDeviceIndex] : null;

        public Action<DeviceInformation>? OnDeviceSelected { get; set; }

        private readonly SpriteFont nameFont;
        private readonly SpriteFont valueFont;
        
        private readonly AudioCaptureContext captureContext;
        private readonly List<DeviceInformation> devices;
        private readonly DeviceInformation? defaultDevice;
        private int selectedDeviceIndex;

        private Vector2 nameSize;
        private Vector2 valueSize;
        private string name;

        private string GetDrawText(bool selected)
        {
            if (SelectedDevice == null)
            {
                return "No devices found";
            }

            if (devices.Count == 1)
                return GetLimitedDeviceName(SelectedDevice);

            var builder = new StringBuilder();

            if (selected && selectedDeviceIndex > 0)
                builder.Append("< ");

            builder.Append(GetLimitedDeviceName(SelectedDevice));

            if (selected && selectedDeviceIndex < devices.Count - 1)
                builder.Append(" >");

            return builder.ToString();
        }

        private string GetLimitedDeviceName(DeviceInformation deviceInfo)
        {
            if (deviceInfo.Name.Length > 25)
            {
                return deviceInfo.Name.Substring(0, 25) + "...";
            }

            return deviceInfo.Name;
        }

        public AudioInputSelectField(string name)
        {
            nameFont = JKContentManager.Font.MenuFont;
            valueFont = JKContentManager.Font.MenuFontSmall;
            Name = name;
            this.name = name; // Shut up the compiler about non nullable fields being null
            
            captureContext = new();

            defaultDevice = captureContext.GetDefaultInputDevice();
            devices = captureContext.GetInputDevices().ToList();
            selectedDeviceIndex = 0;
            
            valueSize = valueFont.MeasureString(GetDrawText(selected: true));
        }

        public override void OnDispose()
        {
            captureContext.Dispose();
        }
        
        protected override BTresult MyRun(TickData tickData)
        {
            if (devices.Count == 0)
                return BTresult.Failure;
            
            var padState = MenuController.instance.GetPadState();

            if (padState.left || padState.right)
            {
                MenuController.instance.ConsumePadPresses();

                selectedDeviceIndex += padState.left ? -1 : 1;
                
                if (selectedDeviceIndex < 0)
                {
                    selectedDeviceIndex = 0;
                    return BTresult.Failure;
                }

                if (selectedDeviceIndex >= devices.Count)
                {
                    selectedDeviceIndex = devices.Count - 1;
                    return BTresult.Failure;
                }

                valueSize = valueFont.MeasureString(GetDrawText(selected: true));
                OnDeviceSelected?.Invoke(SelectedDevice!);

                return BTresult.Success;
            }
            
            return BTresult.Failure;
        }

        public void Draw(int x, int y, bool selected)
        {
            string text = GetDrawText(selected);
            TextHelper.DrawString(nameFont, Name, new Vector2(x, y), Color.White, Vector2.Zero);
            TextHelper.DrawString(valueFont, text, new Vector2(x, y + nameSize.Y + 3), selected ? Color.White : Color.LightGray, Vector2.Zero);
        }

        public Point GetSize()
        {
            Vector2 result = Vector2.Zero;
            result.X = MathHelper.Max(nameSize.X, valueSize.X);
            result.Y = nameSize.Y + valueSize.Y + 3;
            return result.ToPoint();
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
            var field = new AudioInputSelectField(fieldName);

            field.OnDeviceSelected += (val) => ValueChanged?.Invoke(val);
            
            return field;
        }
    }
}