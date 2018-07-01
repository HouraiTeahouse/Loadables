using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace HouraiTeahouse.Loadables.AssetBundles {

public class BuildScript {

  public static string OverloadedDevelopmentServerUrl = "";

  public static void BuildAssetBundles(BuildTarget? target = null) {
    var buildTarget = target ?? EditorUserBuildSettings.activeBuildTarget;
    // Choose the output path according to the build target.
    string outputPath = Path.Combine(BundleUtility.AssetBundlesOutputPath,  BundleUtility.GetPlatformName(target));
    if (!Directory.Exists(outputPath))
      Directory.CreateDirectory (outputPath);

    //@TODO: use append hash... (Make sure pipeline works correctly with it.)
    BuildPipeline.BuildAssetBundles (outputPath,
              BuildAssetBundleOptions.ChunkBasedCompression |
              BuildAssetBundleOptions.DeterministicAssetBundle |
              BuildAssetBundleOptions.StrictMode,
              buildTarget);
    CopyAssetBundlesTo(outputPath, Path.Combine(Application.streamingAssetsPath, BundleUtility.AssetBundlesOutputPath));
          AssetDatabase.Refresh();
  }

  public static void WriteServerURL() {
    string downloadUrl;
    if (string.IsNullOrEmpty(OverloadedDevelopmentServerUrl) == false)
      downloadUrl = OverloadedDevelopmentServerUrl;
    else {
        string localIp = "";
      IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
      foreach (IPAddress ip in host.AddressList) {
        if (ip.AddressFamily == AddressFamily.InterNetwork) {
          localIp = ip.ToString();
          break;
        }
      }
      downloadUrl = "http://" + localIp + ":7888/";
    }

    const string assetBundleManagerResourcesDirectory = "Assets/Dependencies/AssetBundleManager/Resources";
    string assetBundleUrlPath = Path.Combine (assetBundleManagerResourcesDirectory, "AssetBundleServerURL.bytes");
    Directory.CreateDirectory(assetBundleManagerResourcesDirectory);
    File.WriteAllText(assetBundleUrlPath, downloadUrl);
    AssetDatabase.Refresh();
  }

  public static void BuildPlayer() {
    var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
    if (outputPath.Length == 0)
      return;

    string[] levels = GetLevelsFromBuildSettings();
    if (levels.Length == 0) {
      Debug.Log("Nothing to build.");
      return;
    }

          var target = EditorUserBuildSettings.activeBuildTarget;
    string targetName = GetBuildTargetName(target);
    if (targetName == null)
      return;

    // Build and copy AssetBundles.
    BuildAssetBundles();
    CopyAssetBundlesTo(Path.Combine(BundleUtility.AssetBundlesOutputPath,  BundleUtility.GetPlatformName(target)),
              Path.Combine(Application.streamingAssetsPath, BundleUtility.AssetBundlesOutputPath));
    AssetDatabase.Refresh();

    BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
    BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
          if (Directory.Exists(outputPath)) {
              foreach (var file in Directory.GetFiles(outputPath, "*.manifest", SearchOption.AllDirectories))
                  File.Delete(file);
          }
  }

  public static string GetBuildTargetName(BuildTarget target) {
      var baseName =
          string.Join(string.Empty,
              PlayerSettings.productName.Split(' ')
                  .Where(s => !string.IsNullOrEmpty(s))
                  .Select(s => s.Substring(0, 1).ToLower())
                      .ToArray());
    switch(target) {
              case BuildTarget.Android :
            return string.Format("/{0}.apk", baseName);
              case BuildTarget.StandaloneWindows:
              case BuildTarget.StandaloneWindows64:
            return string.Format("/{0}.exe", baseName);
              case BuildTarget.StandaloneOSX:
            return string.Format("/{0}.app", baseName);
              case BuildTarget.WebGL:
                  return "";
                  // Add more build targets for your own.
              default:
                  Debug.LogError("Target not implemented.");
                  return null;
    }
  }

  public static void CopyAssetBundlesTo(string src, string dst) {
    // Clear streaming assets folder.
    Directory.CreateDirectory(dst);

    // Setup the source folder for assetbundles.
    var source = Path.Combine(System.Environment.CurrentDirectory, src);
    if (!Directory.Exists(source))
      Debug.Log("No assetBundle output folder, try to build the assetBundles first.");

    // Setup the destination folder for assetbundles.
    if (Directory.Exists(dst))
      FileUtil.DeleteFileOrDirectory(dst);

    FileUtil.CopyFileOrDirectory(source, dst);
          if (Directory.Exists(dst)) {
              foreach (var file in Directory.GetFiles(dst, "*.manifest*", SearchOption.AllDirectories))
                  File.Delete(file);
          }
  }

		static string[] GetLevelsFromBuildSettings() {
		    return (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
		}
	}
}
