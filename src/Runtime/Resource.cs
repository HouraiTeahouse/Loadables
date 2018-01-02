using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HouraiTeahouse.Loadables {

public static class Resource {

  //TODO(james7132): Move this to somewhere more sane
  public const char BundleSeperator = ':';

  static readonly Dictionary<string, object> resources;

  static Resource() {
    resources = new Dictionary<string, object>();
  }

  public static IResource<T> Get<T>(string location) where T : Object {
    object storedResource;
    IResource<T> resource = null;
    if (resources.TryGetValue(location, out storedResource)) {
      resource = storedResource as IResource<T>;
    }
    if (resource == null) {
      resource = CreateResource<T>(location);
      resources.Add(location, resource);
    }
    return resource;
  }

  static IResource<T> CreateResource<T>(string location) where T : Object {
    if (IsBundlePath(location)) {
      var bundlePath = SplitBundlePath(location);
      return new AssetBundleLoadable<T>(bundlePath.Item1, bundlePath.Item2);
    } else {
      return new ResourcesLoadable<T>(location);
    }
  }

  public static bool IsBundlePath(string path) {
    return path != null && path.IndexOf(BundleSeperator) >= 0;
  }

  public static Tuple<string, string> SplitBundlePath(string path) {
    string[] parts = path.Split(BundleSeperator);
    return Tuple.Create(parts[0], parts[1]);
  }

}

}
