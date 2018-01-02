using HouraiTeahouse.Tasks;

namespace HouraiTeahouse.Loadables {

public class SceneLoadable : ILoadable {

  public bool IsLoaded { get; private set; }

  public void Load() {
  }

  public ITask LoadAsync() {
    return Task.Resolved;
  }

  public void Unload() {
  }

}

}
