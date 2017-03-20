using System;
using Microsoft.Xna.Framework.Content;

namespace HelperSuite.ContentLoader
{
    public class ThreadSafeContentManager : ContentManager
    {
        private static readonly object LoadLock = new object();

        public ThreadSafeContentManager(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public ThreadSafeContentManager(IServiceProvider serviceProvider, string rootDirectory)
            : base(serviceProvider, rootDirectory)
        {
        }

        public override T Load<T>(string assetName)
        {
            lock (LoadLock)
            {
                return base.Load<T>(assetName);
            }
        }
    }

}
