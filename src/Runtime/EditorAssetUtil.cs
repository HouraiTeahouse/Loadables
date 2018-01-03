#if UNITY_EDITOR
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace HouraiTeahouse.Loadables {

public static class EditorAssetUtil {

  const string ResourcePath = "Resources/";
  static readonly Regex ResourceRegex = new Regex(".*/Resources/(.*?)\\..*", RegexOptions.Compiled);

  public static T LoadBundledAsset<T>(string bundleName, string assetName) where T : class {
    string[] path = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);
    return LoadAsset<T>(path.FirstOrDefault());
  }

  public static T LoadAsset<T>(string path) where T : class {
    return AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
  }

  public static bool IsResource(Object asset) {
    return IsResourcePath(AssetDatabase.GetAssetPath(asset));
  }

  public static string GetResourcePath(Object asset) {
    string assetPath = AssetDatabase.GetAssetPath(asset);
    if (!IsResourcePath(assetPath)) {
      return string.Empty;
    }
    return ResourceRegex.Replace(assetPath, "$1");
  }

  static bool IsResourcePath(string path) {
    return !string.IsNullOrEmpty(path) && path.Contains(ResourcePath);
  }

}

}
#endif
