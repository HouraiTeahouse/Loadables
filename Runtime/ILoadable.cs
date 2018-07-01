using System.Threading.Tasks;

namespace HouraiTeahouse.Loadables {

public interface ILoadable {

  bool IsLoaded { get; }
  void Load();
  Task LoadAsync();
  void Unload();

}

public interface ILoadable<T> : ILoadable {

  new T Load();
  new Task<T> LoadAsync();

}

}
