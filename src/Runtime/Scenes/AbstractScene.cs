using HouraiTeahouse.Tasks;
using UnityEngine.SceneManagement;

namespace HouraiTeahouse.Loadables {

public abstract class AbstractScene : IScene {

  public virtual bool IsLoaded { get; protected set; }

  public abstract void Load(LoadSceneMode mode = LoadSceneMode.Single);
  public abstract ITask LoadAsync(LoadSceneMode mode = LoadSceneMode.Single);
  public abstract ITask UnloadAsync();

  public void Unload() => UnloadAsync();

  void ILoadable.Load() => Load(LoadSceneMode.Single);
  ITask ILoadable.LoadAsync() => LoadAsync(LoadSceneMode.Single);

}

}
