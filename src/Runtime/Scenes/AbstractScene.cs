using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace HouraiTeahouse.Loadables {

public abstract class AbstractScene : IScene {

  public virtual bool IsLoaded { get; protected set; }

  public abstract void Load(LoadSceneMode mode = LoadSceneMode.Single);
  public abstract Task LoadAsync(LoadSceneMode mode = LoadSceneMode.Single);
  public abstract Task UnloadAsync();

  public void Unload() => UnloadAsync();

  void ILoadable.Load() => Load(LoadSceneMode.Single);
  Task ILoadable.LoadAsync() => LoadAsync(LoadSceneMode.Single);

}

}
