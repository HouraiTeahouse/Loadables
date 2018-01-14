using System.Threading.Tasks;
using UnityEngine;

namespace HouraiTeahouse.Loadables {

public abstract class AbstractAsset<T> : IAsset<T> where T : Object {

  public virtual T Asset { get; protected set; }
  public bool IsLoaded => Asset != null;

  TaskCompletionSource<T> LoadTask;

  public T Load() {
    if (!IsLoaded) {
      Asset = LoadImpl();
    }
    return Asset;
  }

  public Task<T> LoadAsync() {
    if (IsLoaded) {
      return Task.FromResult(Asset);
    }
    if (LoadTask == null) {
      LoadTask = new TaskCompletionSource<T>();
      LoadAsyncImpl().ContinueWith(asset => {
        Asset = asset.Result;
        LoadTask.SetResult(Asset);
        LoadTask = null;
      });
    }
    return LoadTask.Task;
  }

  public void Unload() {
    if (IsLoaded) {
      return;
    }
    Asset = null;
    // Prefabs cannot be unloaded, only destroyed.
    if (Asset is GameObject) {
      Object.Destroy(Asset);
    } else {
      UnloadImpl();
    }
  }

  public abstract T LoadImpl();
  public abstract Task<T> LoadAsyncImpl();
  public abstract void UnloadImpl();

  void ILoadable.Load() => Load();
  Task ILoadable.LoadAsync() =>LoadAsync();

}

}
