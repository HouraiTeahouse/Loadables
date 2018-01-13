using HouraiTeahouse.Tasks;
using HouraiTeahouse.Loadables.AssetBundles;
using System;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HouraiTeahouse.Loadables {

public class AssetBundleScene : AbstractScene {

  public string BundleName { get; }
  public string SceneName { get; }

  public AssetBundleScene(string location) {
    var bundlePath = Bundles.SplitBundlePath(location);
    BundleName = bundlePath.Item1;
    SceneName = bundlePath.Item2;
  }

  public override void Load(LoadSceneMode mode = LoadSceneMode.Single) {
    throw new InvalidOperationException("Cannot synchronously load scenes from Asset Bundles");
  }

  public override ITask LoadAsync(LoadSceneMode mode = LoadSceneMode.Single) {
#if UNITY_EDITOR
    if (AssetBundleManager.SimulateBundles) {
      string[] levelPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(BundleName, SceneName);
      if (levelPaths.Length == 0) {
        //TODO: The error needs to differentiate that an asset bundle name doesn't exist from that there 
        // right scene does not exist in the asset bundle...
        return Task.FromError(new Exception($"There is no scene with name {SceneName} in {BundleName}"));
      }
      return SceneManager.LoadSceneAsync(levelPaths[0], mode).ToTask();
    }
#endif
    return AssetBundleManager.LoadAssetBundleAsync(BundleName)
      .Then(bundle => SceneManager.LoadSceneAsync(SceneName, mode).ToTask());
  }

  public override ITask UnloadAsync() {
    AssetBundleManager.UnloadAssetBundle(BundleName);
    return SceneManager.UnloadSceneAsync(SceneName).ToTask();
  }

}

}
