using HouraiTeahouse.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

namespace HouraiTeahouse.Loadables.AssetBundles {

// Class takes care of loading assetBundle and its dependencies automatically, loading variants automatically.
public static class AssetBundleManager {

#if UNITY_EDITOR
  static bool? _editorSimulation = null;
  const string SimulateAssetBundles = "SimulateAssetBundles";
#endif

  static readonly Regex SeperatorRegex = new Regex(@"[\\]", RegexOptions.Compiled);
  public static readonly Task<BundleManfiestMap> Manifest = new Task<BundleManfiestMap>();
  static readonly Dictionary<string, ITask<LoadedAssetBundle>> AssetBundles = new Dictionary<string, ITask<LoadedAssetBundle>> ();
  static bool _initalized;

  static AssetBundleManager() {
    BaseDownloadingUrl = "";
    ActiveVariants = new string[0];
  }

  // The base downloading url which is used to generate the full downloading url with the assetBundle names.
  public static string BaseDownloadingUrl { get; set; }

  // Variants which is used to define the active variants.
  public static string[] ActiveVariants { get; set; }

#if UNITY_EDITOR
  // Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
  public static bool SimulateBundles {
    get {
      if (_editorSimulation == null) {
        _editorSimulation = EditorPrefs.GetBool(SimulateAssetBundles, true);
      }
      return _editorSimulation.Value;
    }
    set {
      if (value != _editorSimulation) {
        _editorSimulation = value;
        EditorPrefs.SetBool(SimulateAssetBundles, value);
      }
    }
  }
#endif

  static string GetStreamingAssetsPath() {
    if (Application.isEditor) {
      return "file://" +  Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
    }
    if (Application.isMobilePlatform || Application.isConsolePlatform) {
      return Application.streamingAssetsPath;
    }
    return "file://" +  Application.streamingAssetsPath;
  }

  public static void SetSourceAssetBundleDirectory(string relativePath) {
    BaseDownloadingUrl = GetStreamingAssetsPath() + relativePath;
  }

  public static void SetSourceAssetBundleUrl(string absolutePath) {
    BaseDownloadingUrl = absolutePath + BundleUtility.GetPlatformName() + "/";
  }

  public static void SetDevelopmentAssetBundleServer() {
#if UNITY_EDITOR
    // If we're in Editor simulation mode, we don't have to setup a download URL
    if (SimulateBundles) {
      return;
    }
#endif

    var urlFile = Resources.Load("AssetBundleServerURL") as TextAsset;
    string url = (urlFile != null) ? urlFile.text.Trim() : null;
    if (string.IsNullOrEmpty(url)) {
      Debug.LogError("Development Server URL could not be found.");
    } else {
      SetSourceAssetBundleUrl(url);
    }
  }

  // Load AssetBundleManifest.
  public static ITask<BundleManfiestMap> Initialize() {
    if (_initalized) {
      return Manifest;
    }

    var bundleName = BundleUtility.GetPlatformName();
    Debug.Log("Loading Asset Bundle Manifest: " + bundleName);
#if UNITY_EDITOR
    Debug.Log("Simulation Mode: " + (SimulateBundles ? "Enabled" : "Disabled"));
    // If we're in Editor simulation mode, we don't need the manifest assetBundle.
    if (SimulateBundles) {
      return Task.FromResult<BundleManfiestMap>(null);
    }
#endif

    var bundleLoad = LoadAssetBundleRaw(bundleName);
    bundleLoad.Then(bundle => {
      var loadedBundle = new LoadedAssetBundle(new BundleMetadata(bundleName), bundle);
      AssetBundles.Add(bundleName, Task.FromResult(loadedBundle));
    });
    bundleLoad.Then(bundle => bundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest").ToTask())
      .Then(request => Manifest.Resolve(new BundleManfiestMap(request.asset as AssetBundleManifest)));

    _initalized = true;
    return Manifest;
  }

  public static bool IsValidLocalBundle(string path) {
    return File.Exists(BundleUtility.GetLocalBundlePath(path));
  }

  public static ITask<string[]> GetValidBundlePaths(string glob) {
    var regex = new Regex(glob.Replace("*" , "(.*?)"));
    return Initialize().Then(map => {
      return map.BundleNames.Where(name => regex.IsMatch(name)).ToArray();
    });
  }

  // Load AssetBundle and its dependencies.
  public static ITask<LoadedAssetBundle> LoadAssetBundleAsync(string assetBundleName) {
    Debug.Log("Loading Asset Bundle: " + assetBundleName);

#if UNITY_EDITOR
    // If we're in Editor simulation mode, we don't have to really load the assetBundle and its dependencies.
    if (SimulateBundles) {
      return Task.FromResult<LoadedAssetBundle>(null);
    }
#endif

    return LoadDependencies(assetBundleName)
      .Then(deps => LoadAssetBundleInternal(assetBundleName));
  }

  // Remaps the asset bundle name to the best fitting asset bundle variant.
  static ITask<string> RemapVariantName(string assetBundleName) {
    if (!_initalized)
        Initialize();
    return Manifest.Then(map => {
        var manifest = map.Manifest;
        string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();
        string[] split = assetBundleName.Split('.');

        int bestFit = int.MaxValue;
        int bestFitIndex = -1;
        // Loop all the assetBundles with variant to find the best fit variant assetBundle.
        for (var i = 0; i < bundlesWithVariant.Length; i++) {
            string[] curSplit = bundlesWithVariant[i].Split('.');
            if (curSplit[0] != split[0])
                continue;

            int found = Array.IndexOf(ActiveVariants, curSplit[1]);

            // If there is no active variant found. We still want to use the first
            if (found == -1)
                found = int.MaxValue - 1;

            if (found < bestFit) {
                bestFit = found;
                bestFitIndex = i;
            }
        }

        if (bestFit == int.MaxValue - 1)
            Debug.LogWarning("Ambigious asset bundle variant chosen because there was no matching active variant: "
                + bundlesWithVariant[bestFitIndex]);

        if (bestFitIndex != -1)
            return bundlesWithVariant[bestFitIndex];
        return assetBundleName;
    });
  }

  static ITask<AssetBundle> LoadAssetBundleRaw(string assetBundleName) {
    var path = BundleUtility.GetLocalBundlePath(assetBundleName);
    return AssetBundle.LoadFromFileAsync(path).ToTask().Then(request => request.assetBundle);
  }

  // Where we actually load the assetbundles from the local disk.
  static ITask<LoadedAssetBundle> LoadAssetBundleInternal(string assetBundleName) {
    // Already loaded.
    var name = SeperatorRegex.Replace(assetBundleName, "/");
    ITask<LoadedAssetBundle> bundle;
    if (AssetBundles.TryGetValue(name, out bundle)) {
      bundle.Then(b => b.ReferencedCount++);
      return bundle;
    }
    string path = null;
    var task = Manifest.Then(manifest => {
      if (manifest == null) return null;
      path = manifest[name].Paths.FirstOrDefault(IsValidLocalBundle);
      if (path != null) return LoadAssetBundleRaw(path);
      var message = $"No valid path for asset bundle {name} could be found.";
      throw new FileNotFoundException(message);
    }).Then(assetBundle => {
      if (assetBundle == null) {
        throw new Exception($"{name} is not a valid asset bundle.");
      }
      Debug.Log($"Loaded bundle {name} from {path}.");
      return new LoadedAssetBundle(Manifest.Result[name], assetBundle);
    });
    AssetBundles.Add(name, task);
    return task;
  }

  // Where we get all the dependencies and load them all.
  static ITask<LoadedAssetBundle[]> LoadDependencies(string assetBundleName) {
      return Initialize().ThenAll(map =>
          // Get dependecies from the AssetBundleManifest object..
          map[assetBundleName].Dependencies.Select(dep =>
                  RemapVariantName(dep.Name).Then(name =>
                  LoadAssetBundleInternal(name)))
      );
  }

  // Unload assetbundle and its dependencies.
  public static void UnloadAssetBundle(string assetBundleName) {
#if UNITY_EDITOR
    // If we're in Editor simulation mode, we don't have to load the manifest assetBundle.
    if (SimulateBundles) {
      return;
    }
#endif
    UnloadAssetBundleInternal(assetBundleName);
    UnloadDependencies(assetBundleName);
  }

  static ITask UnloadDependencies(string assetBundleName) {
    return Initialize().Then(map => {
      BundleMetadata bundle;
      if (!map.TryGetValue(assetBundleName, out bundle)) {
        return;
      }

      // Loop dependencies.
      foreach(var dependency in bundle.Dependencies) {
        UnloadAssetBundleInternal(dependency.Name);
      }
    });
  }

  static void UnloadAssetBundleInternal(string assetBundleName) {
    ITask<LoadedAssetBundle> task;
    if (!AssetBundles.TryGetValue(assetBundleName, out task)) {
      return;
    }
    task.Then(bundle => {
      if (bundle == null || --bundle.ReferencedCount != 0) {
        return;
      }
      bundle.AssetBundle.Unload(false);
      AssetBundles.Remove(assetBundleName);
      Debug.Log($"{assetBundleName} has been unloaded successfully");
    });
  }

}

}
