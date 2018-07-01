using HouraiTeahouse.Loadables.AssetBundles;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HouraiTeahouse.Loadables {

public class AssetBundleScene : AbstractScene {

  public override bool IsLoaded => IsSceneLoaded(SceneName);

  public string BundleName { get; }
  public string SceneName { get; }

  public AssetBundleScene(string location) {
    var bundlePath = Bundles.SplitBundlePath(location);
    BundleName = bundlePath.Item1;
    SceneName = bundlePath.Item2;
  }

  protected override void LoadImpl(LoadSceneMode mode = LoadSceneMode.Single) {
    throw new InvalidOperationException("Cannot synchronously load scenes from Asset Bundles");
  }

  protected override async Task LoadAsyncImpl(LoadSceneMode mode = LoadSceneMode.Single) {
#if UNITY_EDITOR
    if (AssetBundleManager.SimulateBundles) {
      // TODO(james7132): Figure out a way to do this without needing the scene in the build settings
      await SceneManager.LoadSceneAsync(SceneName, mode).ToTask();
    }
#endif
    await AssetBundleManager.LoadAssetBundleAsync(BundleName);
    await SceneManager.LoadSceneAsync(SceneName, mode).ToTask();
  }

  public override async Task UnloadAsync() {
    AssetBundleManager.UnloadAssetBundle(BundleName);
    await SceneManager.UnloadSceneAsync(SceneName).ToTask();
  }

}

}
