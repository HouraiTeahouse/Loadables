using HouraiTeahouse.Tasks;

namespace HouraiTeahouse.Loadables {

public interface ILoadable {

  bool IsLoaded { get; }
  void Load();
  ITask LoadAsync();
  void Unload();

}

public interface ILoadable<T> : ILoadable {

  new T Load();
  new ITask<T> LoadAsync();

}

}
