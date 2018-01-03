using HouraiTeahouse.Tasks;
using UnityEngine.SceneManagement;

namespace HouraiTeahouse.Loadables {

public interface IScene : ILoadable {

  void Load(LoadSceneMode mode = LoadSceneMode.Single);
  ITask LoadAsync(LoadSceneMode mode = LoadSceneMode.Single);

  ITask UnloadAsync();

}

}
