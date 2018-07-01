using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HouraiTeahouse.Loadables {

public static class Asset {

  static readonly Dictionary<string, object> assets;

  static Asset() {
    assets = new Dictionary<string, object>();
  }

  public static IAsset<T> Get<T>(string location) where T : Object {
    object storedAsset;
    IAsset<T> asset = null;
    if (assets.TryGetValue(location, out storedAsset)) {
      asset = storedAsset as IAsset<T>;
      if (asset == null) {
        throw new InvalidOperationException("Cannot attempt to load a resource as multiple different types");
      }
    }
    if (asset == null) {
      asset = CreateAsset<T>(location);
      assets.Add(location, asset);
    }
    return asset;
  }

  static IAsset<T> CreateAsset<T>(string location) where T : Object {
    if (Bundles.IsBundlePath(location)) {
      return new AssetBundleAsset<T>(location);
    } else {
      return new ResourcesAsset<T>(location);
    }
  }

}

}
