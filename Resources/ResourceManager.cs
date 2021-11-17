using System.IO;
using System.Reflection;
using System.Text;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Styles;

namespace Resources
{
    public static class ResourceManager
    {
        private static readonly Assembly Assembly;
        
        static ResourceManager()
        {
            Assembly = Assembly.GetExecutingAssembly();
        }
        
        /// <summary>
        /// Loads a resource from the Resources assembly from the given path.
        /// </summary>
        /// <param name="resourcePath">The path to load a resource from. Example: Fonts/MyFont.ttf</param>
        /// <returns></returns>
        public static Stream GetResourceStream(string resourcePath)
        {
            return Assembly.GetManifestResourceStream(GetResourceName(resourcePath));
        }

        /// <summary>
        /// Loads a resource from the Resources assembly and returns the contents as a string.
        /// </summary>
        /// <param name="resourcePath">The path to load a resource from. Example: UI/MyLayout.xmmp</param>
        /// <returns></returns>
        public static string GetResourceString(string resourcePath)
        {
            var bytes = GetResourceBytes(resourcePath);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Loads a resource from the Resources assembly and returns the contents as a byte array.
        /// </summary>
        /// <param name="resourcePath">The path to load a resource from. Example: Sounds/MySound.wav</param>
        /// <returns></returns>
        public static byte[] GetResourceBytes(string resourcePath)
        {
            using var stream = GetResourceStream(resourcePath);
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            
            return bytes;
        }

        /// <summary>
        /// Loads a resource from the Resources assembly and parses the contents as a Widget using <see cref="Project.LoadObjectFromXml{T}"/>.
        /// </summary>
        /// <param name="path">The path to load the resource from. Example: UI/MyLayout.xmmp</param>
        /// <param name="typeResolver">The type resolver to use for custom widgets.</param>
        /// <param name="stylesheet">The stylesheet used to style the widget.</param>
        /// <param name="handler">The handler object to use for event callbacks. Look at the Myra documentation for more information.</param>
        /// <typeparam name="T">The root type of the widget in the loaded xml file.</typeparam>
        /// <typeparam name="THandler">The type of the handler object.</typeparam>
        public static T GetResourceWidget<T, THandler>(string path, ITypeResolver? typeResolver = null, Stylesheet? stylesheet = null, THandler? handler = null) where T : Widget where THandler : class
        {
            var xml = GetResourceString(path);
            return (T)Project.LoadObjectFromXml(xml, new ResourcesAssetManager(), stylesheet ?? Stylesheet.Current, handler, typeResolver);
        }

        private static string GetResourceName(string path)
        {
            path = path.Replace("/", ".").Replace("\\", ".");
            path = "Resources." + path;
            return path;
        }
    }
}