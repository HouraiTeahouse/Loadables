using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace HouraiTeahouse.Loadables {

public class BuiltinScene : AbstractScene {

  public override bool IsLoaded { get; protected set; }

  public string Path { get; }

  public BuiltinScene(string path) {
    Path = path;
  }

  public override void Load(LoadSceneMode mode = LoadSceneMode.Single) {
    SceneManager.LoadScene(Path, mode);
  }

  public override async Task LoadAsync(LoadSceneMode mode = LoadSceneMode.Single) {
    await SceneManager.LoadSceneAsync(Path, mode).ToTask();
  }

  public override async Task UnloadAsync() {
    await SceneManager.UnloadSceneAsync(Path).ToTask();
  }

}

}
