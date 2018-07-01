using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace HouraiTeahouse.Loadables {

public abstract class AbstractScene : IScene {

  public virtual bool IsLoaded { get; }

  public void Load(LoadSceneMode mode = LoadSceneMode.Single) {
    if (!IsLoaded) {
      LoadImpl(mode);
    }
  }

  public Task LoadAsync(LoadSceneMode mode = LoadSceneMode.Single) {
    return !IsLoaded ? LoadAsyncImpl(mode) : Task.CompletedTask;
  }

  protected abstract void LoadImpl(LoadSceneMode mode = LoadSceneMode.Single);
  protected abstract Task LoadAsyncImpl(LoadSceneMode mode = LoadSceneMode.Single);
  public abstract Task UnloadAsync();

  public void Unload() => UnloadAsync();

  protected static bool IsSceneLoaded(string scenePath) {
    for (var i = 0; i < SceneManager.sceneCount; i++) {
      var scene = SceneManager.GetSceneAt(i);
      if (scenePath == scene.path) return true;
      if (scenePath == scene.name) return true;
    }
    return false;
  }

  void ILoadable.Load() => Load(LoadSceneMode.Single);
  Task ILoadable.LoadAsync() => LoadAsync(LoadSceneMode.Single);

}

}
