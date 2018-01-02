using HouraiTeahouse.Tasks;
using UnityEngine;

namespace HouraiTeahouse.Loadables {

public class ResourcesLoadable<T> : AbstractResource<T> where T : Object {

  public string Path { get; }

  public ResourcesLoadable(string path) {
    Path = path;
  }

  public override T LoadImpl() {
    return Resources.Load<T>(Path);
  }

  public override ITask<T> LoadAsyncImpl() {
    return Resources.LoadAsync(Path).ToTask().Then(request => {
      return request.asset as T;
    });
  }

  public override void UnloadImpl() {
    Resources.UnloadAsset(Asset);
  }

}

}
