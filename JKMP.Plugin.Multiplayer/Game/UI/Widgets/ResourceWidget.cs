using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Styles;
using Resources;

namespace JKMP.Plugin.Multiplayer.Game.UI.Widgets
{
    public interface IResourceWidget
    {
        Widget Root { get; }
    }
    
    public class ResourceWidget<T> : Panel, IResourceWidget where T : Widget
    {
        public Widget Root { get; }
        
        public ResourceWidget(string xmmpFilePath)
        {
            Root = AddChild(ResourceManager.GetResourceWidget<Widget, T>(xmmpFilePath, UIManager.TypeResolver, Stylesheet.Current, this as T));
        }

        /// <summary>
        /// Gets a widget by id from the loaded resource widget.
        /// </summary>
        /// <param name="id">The id of the widget.</param>
        /// <typeparam name="TWidgetType">The type of the widget.</typeparam>
        protected TWidgetType EnsureWidgetById<TWidgetType>(string id) where TWidgetType : Widget
        {
            return (TWidgetType)Root.EnsureWidgetById(id);
        }
    }
}