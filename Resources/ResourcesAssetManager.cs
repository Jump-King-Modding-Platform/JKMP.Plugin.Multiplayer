using System.Reflection;
using Myra.Assets;

namespace Resources
{
    public class ResourcesAssetManager : AssetManager
    {
        public ResourcesAssetManager() : base(new ResourceAssetResolver(Assembly.GetExecutingAssembly(), prefix: string.Empty))
        {
        }
    }
}