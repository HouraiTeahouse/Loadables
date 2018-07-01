using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouraiTeahouse.Loadables {

public static class ILoadableExtensions {

  public static void LoadAll(this IEnumerable<ILoadable> loadables) {
    foreach (var loadable in loadables) {
      loadable?.Load();
    }
  }

  public static async Task LoadAllAsync(this IEnumerable<ILoadable> loadables) {
    await Task.WhenAll(loadables.Select(loadable => loadable?.LoadAsync()));
  }

  public static IEnumerable<T> LoadAll<T>(this IEnumerable<ILoadable<T>> loadables) {
    return loadables.Select(loadable => loadable.Load());
  }

  public static Task<T[]> LoadAllAsync<T>(this IEnumerable<ILoadable<T>> loadables) {
    return Task.WhenAll(loadables.Select(loadable => loadable?.LoadAsync()));
  }

  public static void UnloadAll(this IEnumerable<ILoadable> loadables) {
    foreach (var loadable in loadables) {
      loadable?.Unload();
    }
  }

}

}
