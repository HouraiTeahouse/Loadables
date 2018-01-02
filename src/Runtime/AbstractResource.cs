using HouraiTeahouse.Tasks;
using UnityEngine;

namespace HouraiTeahouse.Loadables {

public abstract class AbstractResource<T> : IResource<T> where T : Object {

  public virtual T Asset { get; protected set; }
  public bool IsLoaded => Asset != null;

  ITask<T> LoadTask;

  public T Load() {
    if (!IsLoaded) {
      Asset = LoadImpl();
    }
    return Asset;
  }

  public ITask<T> LoadAsync() {
    if (LoadTask != null) {
      return LoadTask;
    }
    if (IsLoaded) {
      return Task.FromResult(Asset);
    }
    LoadTask = LoadAsyncImpl().Then(asset => {
      Asset = asset;
      LoadTask = null;
      return Asset;
    });
    return LoadTask;
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
  public abstract ITask<T> LoadAsyncImpl();
  public abstract void UnloadImpl();

  void ILoadable.Load() => Load();
  ITask ILoadable.LoadAsync() =>LoadAsync();

}

}
