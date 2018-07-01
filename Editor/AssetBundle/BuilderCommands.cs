using UnityEditor;
using UnityEngine;

namespace HouraiTeahouse.Loadables.AssetBundles {

public static class BuilderCommands {

  [MenuItem("Hourai Teahouse/Build/Build Asset Bundles (Windows)")]
  public static void BuildAssetBundlesWindows() {
    BuildScript.BuildAssetBundles(BuildTarget.StandaloneWindows64);
  }

  [MenuItem("Hourai Teahouse/Build/Build Asset Bundles (Mac OSX)")]
  public static void BuildAssetBundlesOSX() {
    BuildScript.BuildAssetBundles(BuildTarget.StandaloneOSX);
  }

  [MenuItem("Hourai Teahouse/Build/Build Asset Bundles (Linux)")]
  public static void BuildAssetBundlesLinux() {
    BuildScript.BuildAssetBundles(BuildTarget.StandaloneLinuxUniversal);
  }

}

}

