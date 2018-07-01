using UnityEngine;

namespace HouraiTeahouse.Loadables.AssetBundles {

    /// <summary>
    // Loaded assetBundle contains the references count which can be used to unload dependent assetBundles automatically.
    /// </summary>
    public class LoadedAssetBundle {

        /// <summary>
        /// The reference to the loaded AssetBundle itself.
        /// </summary>
        public AssetBundle AssetBundle { get; private set; }

        /// <summary>
        /// Gets readonly metadata about the asset bundle.
        /// </summary>
        public BundleMetadata Metadata { get; private set; }

        /// <summary>
        /// The number of times this bundle has been referenced.
        /// </summary>
        public int ReferencedCount { get; internal set; }

        internal LoadedAssetBundle(BundleMetadata metadata, AssetBundle assetBundle) {
            Metadata = Argument.NotNull(metadata);
            AssetBundle = Argument.NotNull(assetBundle);
            ReferencedCount = 1;
        }

    }

}
