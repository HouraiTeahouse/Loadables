using HouraiTeahouse.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace HouraiTeahouse.Loadables {

public static class ILoadableExtensions {

  public static void LoadAll(this IEnumerable<ILoadable> loadables) {
    foreach (var loadable in loadables) {
      loadable?.Load();
    }
  }

  public static ITask LoadAllAsync(this IEnumerable<ILoadable> loadables) {
    return Task.All(loadables.Select(loadable => loadable?.LoadAsync()));
  }

  public static IEnumerable<T> LoadAll<T>(this IEnumerable<ILoadable<T>> loadables) {
    return loadables.Select(loadable => loadable.Load());
  }

  public static ITask<T[]> LoadAllAsync<T>(this IEnumerable<ILoadable<T>> loadables) {
    return Task.All(loadables.Select(loadable => loadable?.LoadAsync()));
  }

  public static void UnloadAll(this IEnumerable<ILoadable> loadables) {
    foreach (var loadable in loadables) {
      loadable?.Unload();
    }
  }

}

}
