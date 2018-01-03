using HouraiTeahouse.Tasks;
using HouraiTeahouse.Loadables.AssetBundles;
using System;
using UnityEngine.SceneManagement;

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
    return AssetBundleManager.LoadLevelAsync(BundleName, SceneName, mode);
  }

  public override ITask UnloadAsync() {
    return SceneManager.UnloadSceneAsync(SceneName).ToTask();
  }

}

}
