# Loadables

A Unity3D library for simplifying and abstracting the processes of dynamically
loading assets from game data stores like Resources of AssetBundles.

## Setup
We generally advise using git submodules to include this into any git based project.
If that is not possible, either clone or download the source and include it anywhere
your Unity Assets folder.

This library has a dependency on [Tasks](https://github.com/HouraiTeahouse/Tasks).
Be sure to add it alongside this library.

TODO: Create a \*.unitypackage.

## The ILoadable Interface
The library centers around an the `ILoadable` and `ILoadable<T>` interfaces,
which represents or wraps a loadable resource and manages its state in memory.
These interfaces offer these main functions:

 * `IsLoaded` - is the resource wrapped by this Loadable currently loaded in
   memory?
 * `Load()` - Loadss a currently unloaded resource into memory. This is a
   synchronous operation and will block the executing thread until it is
   complete. Should be a no-op if the resource is already loaded.
 * `LoadAsync()` - Same as Load(), but executes it asynchronously, returns a
   `ITask` or `ITask<T>` that represents the load operation that will be
   resolved when completed.
 * `Unload() `- Unloads a loaded resource from memory.

## Note about Asset Bundles
Currently this library does not support loading remote Asset Bundles. The only
supported AssetBundle usecase is loading them from the local StreamingAssets.

## Asset Loadables
In Unity, assets can be loaded dynamically from Resources or AssetBundles. This
library abstracts that away using the `Asset` class.

The Asset class has a single method `Asset.Get<T>(string assetPath)`. This
returns a `IAsset<T>`, a Loadable representing the asset.

**Resource Asset Paths** - Normal Resources based assets use the normal Resource
path, the same ones accepted by `Resources.Load`.

**Asset Bundle Paths** -Asset bundle paths are expected to come in the form of
`<asset bundle name>:<asset name>`.

Examples:

```csharp
// Get Asset wrapper.
IAsset<Sprite> spriteResource = Asset.Get<Sprite>("Resource/Path/To/Sprite");

// Load asset synchronously. Note: this only works for loading from Resources.
// AssetBbundle loads do not support loading synchronously.
Sprite loadedSprite = spriteResource.Load();

// Load asset asynchronously.
ITask<Sprite> loadTask = spriteResource.LoadAsync();

// Add callback for handling the asset after loading.
loadTask.Then(sprite => {
  loadedSprite = sprite;
});
```

A few things to keep in mind when working with asset loadables:

 * Loading and unmloading of AssetBundles is automatically handled.
 * Calling `Asset.Get<T>` repetedly with the same argument will always return
   the same `IAsset<T>` instance for a given asset path.
 * Attempting to create an `IAsset<T>` of different types for the same path will
   result in an error. For example, calling `Assets.Get<GameObject>("Same/Path")`
   then seperately calling `Assets.Get<Sprite>("Same/Path")` will result in the
   second call generating an error.

## Scene Loadables
In Unity, scenes can are loaded as a part of built in scenes or from AssetBundles.
This library abstracts the difference using the `Scene` class.

The Scene class has a single method `Scene.Get(string scenePath)`. This
returns a `IScene`, a Loadable representing the scene.

**Built in Scenes** - Normal scenes use the normal scene load
path, the same ones accepted by `SceneManager.LoadScene`.

**Asset Bundle Paths** - Asset bundle paths are expected to come in the form of
`<asset bundle name>:<scene name>`.

Examples:

```csharp
// Get Scene wrapper.
IScene scene = Asset.Get<Sprite>("Scene/Path");

// Load scene synchronously. Note: this only works for loading from built in
// scenes. AssetBbundle loads do not support loading synchronously.
//
// IScene accepts an optional LoadSceneMode parameter to change the way the
// scene is loaded. By default, if calling without the parameter, it defaults ot
// LoadSceneMode.Single.
scene.Load();
scene.Load(LoadSceneMode.Additive);

// Load scene asynchronously.
ITask loadTask = scene.LoadAsync();

// Add callback for doing operations after loading the scene.
loadTask.Then(() => {
  Debug.Log("Scene Loaded!");
});
```

A few things to keep in mind when working with asset loadables:

 * Calling `Scene.Get` repetedly with the same argument will always return
   the same `IScene` instance for a given asset path.

## Getting Loadable Paths: Property Attributes
Getting these paths can be quite painful if entering them via the editor.
Loadables provides two PropertyAttributes to simplify getting these pahts. Both
work only on fields of `string` type.

### ResourceAttribute
`ResourceAttribute` allows one to assign an arbitrary resource in a object field
in the Unity Editor and have it's Resoruce or Asset Bundle path saved.

It also supports restricting the type of the object field shown.

If the saved object is not in Resource folder or is not assigned to a
AssetBundle, the field will turn red to indicate that it is not a valid path and
the resource reference will not be saved.

Mousing over the field to show the tooltip of the field will show additional
information regarding the saved object, including the full saved path.

Moving the asset around the Resource folder or changing it's asset bundle will
break the reference to it in the Editor.

Example:

```csharp
// Shows a generic UnityEngine.Object field to save the path to the string.
[Resource] public string ResourceToGet;

// The type of the field can be restricted with a type restriction.
[Resource(typeof(Sprite))] public string IconResource;

// It is common to make the path private and expose a IAsset<T> get-only
// property.
[Resource(typeof(Sprite)), SerializeField]
string _portrait;

public IAsset<Sprite> Portrait => Asset.Get<Sprite>(_portrait);
```

### SceneAttribute
`SceneAttribute` allows one to assign an arbitrary scene in a object field
in the Unity Editor and have it's scene path or Asset Bundle path saved.

If the saved object is not in the build settings or is not assigned to a
AssetBundle, the field will turn red to indicate that it is not a valid path and
the reference will not be saved.

Mousing over the field to show the tooltip of the field will show additional
information regarding the saved scene, including the full saved path.

Moving the scene around or changing it's asset bundle will break the reference
to it in the Editor.

Example:

```csharp
// Shows a generic UnityEngine.Object field to save the path to the string.
[Scene] public string SceneToLoad;

// It is common to make the path private and expose a IScene get-only property.
[Scene, SerializeField]
string _mainScene;

public IScene Portrait => Scene.Get(_portrait);
```
