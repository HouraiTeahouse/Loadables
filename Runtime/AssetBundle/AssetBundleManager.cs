using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
  public static readonly TaskCompletionSource<BundleManfiestMap> Manifest = new TaskCompletionSource<BundleManfiestMap>();
  static readonly Dictionary<string, TaskCompletionSource<LoadedAssetBundle>> AssetBundles = new Dictionary<string, TaskCompletionSource<LoadedAssetBundle>> ();
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

  public static Task<BundleManfiestMap> Initialize() {
    if (_initalized) {
      return Manifest.Task;
    }

#if UNITY_EDITOR
    Debug.Log("Simulation Mode: " + (SimulateBundles ? "Enabled" : "Disabled"));
    // If we're in Editor simulation mode, we don't need the manifest assetBundle.
    if (SimulateBundles) {
      return Task.FromResult<BundleManfiestMap>(null);
    }
#endif

    LoadManifest();

    _initalized = true;
    return Manifest.Task;
  }

  static async void LoadManifest() {
    var bundleName = BundleUtility.GetPlatformName();
    Debug.Log("Loading Asset Bundle Manifest: " + bundleName);
    var bundle = await LoadAssetBundleRaw(bundleName);
    var loadedBundle = new LoadedAssetBundle(new BundleMetadata(bundleName), bundle);
    var task = new TaskCompletionSource<LoadedAssetBundle>(loadedBundle);
    AssetBundles.Add(bundleName, task);
    var request = await bundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest").ToTask();
    Manifest.SetResult(new BundleManfiestMap(request.asset as AssetBundleManifest));
  }

  public static bool IsValidLocalBundle(string path) {
    return File.Exists(BundleUtility.GetLocalBundlePath(path));
  }

  public static async Task<string[]> GetValidBundlePaths(string glob) {
    var regex = new Regex(glob.Replace("*" , "(.*?)"));
    var map = await Initialize();
    return map.BundleNames.Where(name => regex.IsMatch(name)).ToArray();
  }

  // Load AssetBundle and its dependencies.
  public static async Task<LoadedAssetBundle> LoadAssetBundleAsync(string assetBundleName) {
    Debug.Log("Loading Asset Bundle: " + assetBundleName);

#if UNITY_EDITOR
    // If we're in Editor simulation mode, we don't have to really load the assetBundle and its dependencies.
    if (SimulateBundles) {
      return null;
    }
#endif

    await LoadDependencies(assetBundleName);
    return await LoadAssetBundleInternal(assetBundleName);
  }

  // Remaps the asset bundle name to the best fitting asset bundle variant.
  static async Task<string> RemapVariantName(string assetBundleName) {
    var map = await Initialize();
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
  }

  static async Task<AssetBundle> LoadAssetBundleRaw(string assetBundleName) {
    const int kTimeout = 20000;
    var path = BundleUtility.GetLocalBundlePath(assetBundleName);
    var loadTask = AssetBundle.LoadFromFileAsync(path).ToTask();
    if (await Task.WhenAny(loadTask, Task.Delay(kTimeout)) == loadTask) {
      return loadTask.Result.assetBundle;
    } else {
      Debug.LogError($"Asset Bundle Load timed out. Bundle Name: {assetBundleName}. Timeout: {kTimeout}");
      throw new Exception($"Asset Bundle Load timed out. Bundle Name: {assetBundleName}. Timeout: {kTimeout}");
    }
  }

  // Where we actually load the assetbundles from the local disk.
  static async Task<LoadedAssetBundle> LoadAssetBundleInternal(string assetBundleName) {
    // Already loaded.
    var name = SeperatorRegex.Replace(assetBundleName, "/");
    TaskCompletionSource<LoadedAssetBundle> bundle;
    if (AssetBundles.TryGetValue(name, out bundle)) {
      var loadedBundle = await bundle.Task;
      loadedBundle.ReferencedCount++;
      return loadedBundle;
    }
    bundle = new TaskCompletionSource<LoadedAssetBundle>();
    AssetBundles.Add(name, bundle);
    string path = null;
    var map = await Initialize();
    if (map == null) return null;
    path = map[name].Paths.FirstOrDefault(IsValidLocalBundle);
    AssetBundle assetBundle = null;
    if (path != null) {
      assetBundle = await LoadAssetBundleRaw(path);
    } else {
      var message = $"No valid path for asset bundle {name} could be found.";
      throw new FileNotFoundException(message);
    }
    if (assetBundle == null) {
      Debug.LogError($"{name} is not a valid asset bundle.");
      throw new Exception($"{name} is not a valid asset bundle.");
    }
    Debug.Log($"Loaded bundle {name} from {path}.");
    var newBundle = new LoadedAssetBundle(map[name], assetBundle);
    bundle.SetResult(newBundle);
    return newBundle;
  }

  // Where we get all the dependencies and load them all.
  static async Task<LoadedAssetBundle[]> LoadDependencies(string assetBundleName) {
    var map = await Initialize();
    var depTasks = map[assetBundleName].Dependencies.Select(dep => RemapVariantName(dep.Name));
    var dependencies = await Task.WhenAll(depTasks);
    return await Task.WhenAll(dependencies.Select(dep => LoadAssetBundleInternal(dep)));
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

  static async void UnloadDependencies(string assetBundleName) {
    var map = await Initialize();
    BundleMetadata bundle;
    if (!map.TryGetValue(assetBundleName, out bundle)) {
      return;
    }

    // Loop dependencies.
    foreach(var dependency in bundle.Dependencies) {
      UnloadAssetBundleInternal(dependency.Name);
    }
  }

  static async void UnloadAssetBundleInternal(string assetBundleName) {
    TaskCompletionSource<LoadedAssetBundle> task;
    if (!AssetBundles.TryGetValue(assetBundleName, out task)) {
      return;
    }
    var bundle = await task.Task;
    if (bundle == null || --bundle.ReferencedCount != 0) {
      return;
    }
    bundle.AssetBundle.Unload(false);
    AssetBundles.Remove(assetBundleName);
    Debug.Log($"{assetBundleName} has been unloaded successfully");
  }

}

}
