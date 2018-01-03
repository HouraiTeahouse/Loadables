using HouraiTeahouse.Tasks;
using UnityEngine.SceneManagement;

namespace HouraiTeahouse.Loadables {

public class BuiltinScene : AbstractScene {

  public bool IsLoaded { get; private set; }

  public string Path { get; }

  public BuiltinScene(string path) {
    Path = path;
  }

  public override void Load(LoadSceneMode mode = LoadSceneMode.Single) {
    SceneManager.LoadScene(Path, mode);
  }

  public override ITask LoadAsync(LoadSceneMode mode = LoadSceneMode.Single) {
    return SceneManager.LoadSceneAsync(Path, mode).ToTask();
  }

  public override ITask UnloadAsync() {
    return SceneManager.UnloadSceneAsync(Path).ToTask();
  }

}

}
