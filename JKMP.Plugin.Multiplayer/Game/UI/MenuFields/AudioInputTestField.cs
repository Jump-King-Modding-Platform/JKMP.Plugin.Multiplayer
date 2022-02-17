using System;
using System.Collections.Generic;
using System.Reflection;
using BehaviorTree;
using JKMP.Core.Configuration.Attributes;
using JKMP.Core.Configuration.Attributes.PropertyCreators;
using JumpKing.PauseMenu;
using JumpKing.PauseMenu.BT;
using Microsoft.Xna.Framework;
using IDrawable = JumpKing.Util.IDrawable;

namespace JKMP.Plugin.Multiplayer.Game.UI.MenuFields
{
    internal class AudioInputTestField : IBTnode, IMenuItem
    {
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
            return new TextInfo("AudioInputTestField", Color.White);
        }
    }
}