using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HouraiTeahouse.Loadables {

public static class Scene {

  static readonly IDictionary<string, IScene> scenes;

  static Scene() {
    scenes = new Dictionary<string, IScene>();
  }

  public static IScene Get(string location) {
    IScene scene;
    if (!scenes.TryGetValue(location, out scene)) {
      scene = CreateScene(location);
      scenes.Add(location, scene);
    }
    return scene;
  }

  static IScene CreateScene(string location) {
    if (Bundles.IsBundlePath(location)) {
      return new AssetBundleScene(location);
    } else {
      return new BuiltinScene(location);
    }
  }

}

}
