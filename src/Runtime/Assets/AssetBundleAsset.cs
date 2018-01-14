using HouraiTeahouse.Loadables.AssetBundles;
using System;
using System.Threading.Tasks;
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
    }
#endif
    throw new InvalidOperationException($"Cannot synchronously load assets from AssetBundles. Path: {Path}");
  }

  public override async Task<T> LoadAsyncImpl() {
    Debug.Log($"Loading {AssetName} from {BundleName} bundle...");
#if UNITY_EDITOR
    if (AssetBundleManager.SimulateBundles || !EditorApplication.isPlayingOrWillChangePlaymode) {
      string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(BundleName, AssetName);
      if (assetPaths.Length != 0) {
        return AssetDatabase.LoadAssetAtPath<T>(assetPaths[0]);
      }
      throw new Exception($"There is no asset with name {AssetName} in {BundleName}");
    }
#endif
    var bundle = await AssetBundleManager.LoadAssetBundleAsync(BundleName);
    var request = await bundle.AssetBundle.LoadAssetAsync<T>(AssetName).ToTask();
    Debug.Log($"Loaded {AssetName} from {BundleName}");
    return request.asset as T;
  }

  public override void UnloadImpl() {
    Resources.UnloadAsset(Asset);
    AssetBundleManager.UnloadAssetBundle(BundleName);
  }

}

}
