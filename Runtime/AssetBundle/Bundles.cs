using System;

namespace HouraiTeahouse.Loadables {

public static class Bundles {

  public const char PathSeperator = ':';

  public static string CreateBundlePath(string bundleName, string bundleResource) {
    return bundleName + PathSeperator + bundleResource;
  }

  public static bool IsBundlePath(string path) {
    return path != null && path.IndexOf(PathSeperator) >= 0;
  }

  public static Tuple<string, string> SplitBundlePath(string path) {
    string[] parts = path.Split(PathSeperator);
    return Tuple.Create(parts[0], parts[1]);
  }

}

}
