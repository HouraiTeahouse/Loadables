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
    if (location.IndexOf(BundleSeperator) >= 0) {
      string[] parts = location.Split(BundleSeperator);
      return new AssetBundleLoadable<T>(parts[0], parts[1]);
    } else {
      return new ResourcesLoadable<T>(location);
    }
  }

}

}
