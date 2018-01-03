using HouraiTeahouse.Tasks;
using HouraiTeahouse.Loadables.AssetBundles;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HouraiTeahouse.Loadables {

public class AssetBundleAsset<T> : AbstractAsset<T> where T : UnityEngine.Object {

  public string BundleName { get; }
  public string AssetName { get; }

  public string Path => Bundles.CreateBundlePath(BundleName, AssetName);

  public AssetBundleAsset(string path) {
    var bundlePath = Bundles.SplitBundlePath(path);
    BundleName = bundlePath.Item1;
    AssetName = bundlePath.Item2;
  }

  public override T LoadImpl() {
#if UNITY_EDITOR
    if (!EditorApplication.isPlayingOrWillChangePlaymode) {
      return EditorAssetUtil.LoadBundledAsset<T>(BundleName, AssetName);
    } else
#endif
    {
      throw new InvalidOperationException($"Cannot synchronously load assets from AssetBundles. Path: {Path}");
    }
  }

  public override ITask<T> LoadAsyncImpl() {
    return AssetBundleManager.LoadAssetAsync<T>(BundleName, AssetName);
  }

  public override void UnloadImpl() {
    Resources.UnloadAsset(Asset);
    AssetBundleManager.UnloadAssetBundle(BundleName);
  }

}

}
