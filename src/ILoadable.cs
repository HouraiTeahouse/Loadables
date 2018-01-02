using HouraiTeahouse.Tasks;

namespace HouraiTeahouse.Loadables {

public interface ILoadable {

  void Load();
  ITask LoadAsync();

}

public interface ILoadable<T> : ILoadable {

  new T Load();
  new ITask<T> LoadAsync();

}

}
