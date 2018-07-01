using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace HouraiTeahouse.Loadables {

public interface IScene : ILoadable {

  void Load(LoadSceneMode mode = LoadSceneMode.Single);
  Task LoadAsync(LoadSceneMode mode = LoadSceneMode.Single);

  Task UnloadAsync();

}

}
