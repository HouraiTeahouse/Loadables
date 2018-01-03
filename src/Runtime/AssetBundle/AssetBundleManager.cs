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
  static int _simulateAssetBundleInEditor = -1;
  const string SimulateAssetBundles = "SimulateAssetBundles";
#endif

  static readonly Regex SeperatorRegex = new Regex(@"[\\]", RegexOptions.Compiled);
  public static readonly Task<BundleManfiestMap> Manifest = new Task<BundleManfiestMap>();
  static readonly Dictionary<string, ITask<LoadedAssetBundle>> AssetBundles = new Dictionary<string, ITask<LoadedAssetBundle>> ();
  static readonly Dictionary<Type, Delegate> TypeHandlers = new Dictionary<Type, Delegate> ();
  static bool _initalized;

  public static void AddHandler<T>(Action<T> handler) {
    var type = typeof(T);
    Delegate current;
    TypeHandlers[type] = TypeHandlers.TryGetValue(type, out current) ? Delegate.Combine(current, handler) : handler;
  }

  public static void RemoveHandler<T>(Action<T> handler) {
    TypeHandlers[typeof(T)] = Delegate.RemoveAll(TypeHandlers[typeof(T)], handler);
  }

  public static void CopyDirectory(string src, string dest) {
    if (src == dest) {
        return;
    }
    //Now Create all of the directories
    foreach (string dirPath in Directory.GetDirectories(src, "*", SearchOption.AllDirectories)) {
      var dir = dirPath.Replace(src, dest);
      if (!Directory.Exists(dir)) {
        Directory.CreateDirectory(dir);
      }
    }

    //Copy all the files & Replaces any files with the same name
    foreach (string newPath in Directory.GetFiles(src, "*", SearchOption.AllDirectories)) {
      var file = newPath.Replace(src, dest);
      if (!File.Exists(file)) {
        File.Copy(newPath, file, true);
      }
    }
  }

    //TODO(james7132): Move this somewhere more sane
    //public static ITask LoadLocalBundles(IEnumerable<string> whitelist, IEnumerable<string> blacklist = null) {
        //if (whitelist == null || blacklist == null || !whitelist.Any())
            //return Task.Resolved;
        //string basePath = BundleUtility.GetLocalBundlePath("");
        //Debug.LogFormat("Loading local asset bundles from {0}....", basePath);
        //var whitelistRegex = whitelist
            //.Select(r => new Regex(r.Replace("*", "(.*?)"),
            //RegexOptions.Compiled)).ToArray();
        //var blacklistRegex = blacklist
            //.Select(r => new Regex(r.Replace("*", "(.*?)"),
            //RegexOptions.Compiled)).ToArray();
        //var task = Initialize().Then(map =>
            //Task.All(map.BundleNames
                //.Where(name => blacklistRegex.All(r => !r.IsMatch(name)) && whitelistRegex.Any(r => r.IsMatch(name)))
                //.Select(name => LoadAssetBundleAsync(name).Then(loadedBundle => {
                    //var bundle = loadedBundle.AssetBundle;
                    //if (bundle == null) {
                        //Debug.LogErrorFormat("Failed to load an AssetBundle from {0}.", loadedBundle.Metadata.Name);
                        //return Task.Resolved;
                    //}
                    //var assetName = bundle.GetAllAssetNames()[0];
                    //ITask<Object> assetTask = bundle.mainAsset != null
                        //? Task.FromResult(bundle.mainAsset)
                        //: LoadAssetAsync<Object>(loadedBundle.Metadata.Name, assetName);
                    //return assetTask.Then(asset => {
                        //if (asset == null)
                            //return;
                        //var assetType = asset.GetType();
                        //Delegate handler;
                        //if (TypeHandlers.TryGetValue(assetType, out handler)) {
                            //Debug.LogFormat("Loaded {0} ({1}) from \"{2}\".", asset.name, assetType.Name, loadedBundle.Metadata.Name);
                            //handler.DynamicInvoke(asset);
                        //} else {
                            //Debug.LogErrorFormat(
                                //"Attempted to load an asset of type {0} from asset bundle {1}, but no handler was found. Unloading...",
                                //assetType,
                                //loadedBundle.Metadata.Name);
                            //UnloadAssetBundle(loadedBundle.Metadata.Name);
                        //}
                //});
            //}))
        //));
        //task.Then(() => Debug.Log("Done loading local asset bundles"));
        //return task;
    //}

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
  public static bool SimulateAssetBundleInEditor {
    get {
      if (_simulateAssetBundleInEditor == -1)
        _simulateAssetBundleInEditor = EditorPrefs.GetBool(SimulateAssetBundles, true) ? 1 : 0;

      return _simulateAssetBundleInEditor != 0;
    }
    set {
      int newValue = value ? 1 : 0;
        if (newValue == _simulateAssetBundleInEditor)
            return;
        _simulateAssetBundleInEditor = newValue;
        EditorPrefs.SetBool(SimulateAssetBundles, value);
    }
  }
#endif

  static string GetStreamingAssetsPath() {
      if (Application.isEditor)
    return "file://" +  Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
      if (Application.isMobilePlatform || Application.isConsolePlatform)
          return Application.streamingAssetsPath;
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
    if (SimulateAssetBundleInEditor){
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
  public static ITask<BundleManfiestMap> Initialize () {
    if (_initalized) {
      return Manifest;
    }
#if UNITY_EDITOR
    Debug.Log("Simulation Mode: " + (SimulateAssetBundleInEditor ? "Enabled" : "Disabled"));
    // If we're in Editor simulation mode, we don't need the manifest assetBundle.
    if (SimulateAssetBundleInEditor) {
      return Task.FromResult<BundleManfiestMap>(null);
    }
#endif

    var task = LoadAssetAsync<AssetBundleManifest>(BundleUtility.GetPlatformName(), "AssetBundleManifest");
        task.Then(manifest => Manifest.Resolve(new BundleManfiestMap(manifest)));
    _initalized = true;
    return Manifest;
  }

  // Load AssetBundle and its dependencies.
  static ITask<LoadedAssetBundle> LoadAssetBundleAsync(string assetBundleName, bool isLoadingAssetBundleManifest = false) {
    Debug.Log("Loading Asset Bundle" + (isLoadingAssetBundleManifest ? " Manifest: " : ": ") + assetBundleName);

#if UNITY_EDITOR
    // If we're in Editor simulation mode, we don't have to really load the assetBundle and its dependencies.
    if (SimulateAssetBundleInEditor) {
      return Task.FromResult<LoadedAssetBundle>(null);
    }
#endif

    var mainTask = LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);
    if (isLoadingAssetBundleManifest) {
      return mainTask;
    }
    return LoadDependencies(assetBundleName).Then(deps => mainTask);
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

  // Where we actually load the assetbundles from the local disk.
  static ITask<LoadedAssetBundle> LoadAssetBundleInternal(string assetBundleName, bool isManifest) {
    // Already loaded.
    var name = SeperatorRegex.Replace(assetBundleName, "/");
    ITask<LoadedAssetBundle> bundle;
    if (AssetBundles.TryGetValue(name, out bundle)) {
      bundle.Then(b => b.ReferencedCount++);
      return bundle;
    }

    ITask<string> pathTask;
    if (isManifest) {
      pathTask = Task.FromResult(BundleUtility.GetLocalBundlePath(name));
    } else {
        pathTask = Manifest.Then(manfiest => {
          if (manfiest == null) {
            return null;
          }
          foreach (var path in manfiest[name].Paths) {
            var fullPath = BundleUtility.GetLocalBundlePath(path);
            if (File.Exists(fullPath)) {
              return fullPath;
            }
          }
          var message = string.Format("No valid path for asset bundle {0} could be found.", name);
          throw new FileNotFoundException(message);
      });
    }
    // For manifest assetbundle, always download it as we don't have hash for it.
    var task = pathTask.Then(path => {
        var operation = AssetBundle.LoadFromFileAsync(path);
        return operation.ToTask().Then(request => {
            var assetBundle = request.assetBundle;
            if (assetBundle == null) {
                var message = string.Format("{0} is not a valid asset bundle.", name);
                throw new Exception(message);
            }
            LoadedAssetBundle loadedBundle;
            if (isManifest)
                 loadedBundle = new LoadedAssetBundle(
                     new BundleMetadata(assetBundleName,
                                        new Hash128(),
                                        Enumerable.Empty<BundleMetadata>(),
                                        Enumerable.Empty<string>()),
                     assetBundle);
            else
                loadedBundle = new LoadedAssetBundle(Manifest.Result[name], assetBundle);
            Debug.LogFormat("Loaded bundle \"{0}\" from {1}.", name, path);
            return loadedBundle;
        });
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
                  LoadAssetBundleInternal(name, false)))
      );
  }

  // Unload assetbundle and its dependencies.
  public static void UnloadAssetBundle(string assetBundleName) {
#if UNITY_EDITOR
    // If we're in Editor simulation mode, we don't have to load the manifest assetBundle.
    if (SimulateAssetBundleInEditor) {
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
      Debug.LogFormat("{0} has been unloaded successfully", assetBundleName);
    });
  }

  // Load asset from the given assetBundle.
  public static ITask<T> LoadAssetAsync<T>(string assetBundleName, string assetName) where T : Object {
    Debug.LogFormat("Loading {0} from {1} bundle...", assetName, assetBundleName);

#if UNITY_EDITOR
    if (SimulateAssetBundleInEditor) {
      string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
      if (assetPaths.Length != 0) {
        return Task.FromResult(AssetDatabase.LoadAssetAtPath<T>(assetPaths[0]));
      }
      var message = string.Format("There is no asset with name \"{}\" in {}", assetName, assetBundleName);
      Debug.LogError(message);
      return Task.FromError<T>(new Exception(message));
    }
#endif
    ITask<LoadedAssetBundle> task;
    if (typeof(T) == typeof(AssetBundleManifest)) {
      task = LoadAssetBundleInternal(assetBundleName, true);
    } else {
      task = RemapVariantName(assetBundleName).Then(bundleName => LoadAssetBundleAsync(bundleName));
    }
    var assetTask = task.Then(bundle => bundle.AssetBundle.LoadAssetAsync<T>(assetName).ToTask());
    assetTask.Then(() => Debug.LogFormat("Loaded {0} from {1}", assetName, assetBundleName));
    return assetTask.Then(request => request.asset as T);
  }

  // Load level from the given assetBundle.
  public static ITask LoadLevelAsync (string assetBundleName, string levelName, LoadSceneMode loadMode = LoadSceneMode.Single) {
    Debug.LogFormat("Loading {0} from {1} bundle...", levelName, assetBundleName);

#if UNITY_EDITOR
    if (SimulateAssetBundleInEditor) {
            string[] levelPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, levelName);
            if (levelPaths.Length == 0) {
                //TODO: The error needs to differentiate that an asset bundle name doesn't exist from that there right scene does not exist in the asset bundle...
                return Task.FromError(new Exception("There is no scene with name \"" + levelName + "\" in " + assetBundleName));
            }
        return SceneManager.LoadSceneAsync(levelPaths[0], loadMode).ToTask();
    }
#endif
        return RemapVariantName(assetBundleName)
            .Then(bundleName => LoadAssetBundleAsync(bundleName))
            .Then(bundle => SceneManager.LoadSceneAsync(levelName, loadMode).ToTask());
  }

}

}
