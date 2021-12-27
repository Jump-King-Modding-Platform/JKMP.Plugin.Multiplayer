using Myra.Graphics2D.UI;

namespace JKMP.Plugin.Multiplayer.Game.UI
{
    internal static class MyraExtensions
    {
        public static T? FindParentOfType<T>(this Widget widget) where T : Widget
        {
            Widget current = widget.Parent;

            while (true)
            {
                if (current == null)
                    return null;
                
                if (current.GetType() == typeof(T))
                    return (T)current;

                current = current.Parent;
            }
        }
    }
}