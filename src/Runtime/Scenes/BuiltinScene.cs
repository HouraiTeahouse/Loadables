using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace HouraiTeahouse.Loadables {

public class BuiltinScene : AbstractScene {

  public override bool IsLoaded => IsSceneLoaded(Path);

  public string Path { get; }

  public BuiltinScene(string path) {
    Path = path;
  }

  protected override void LoadImpl(LoadSceneMode mode = LoadSceneMode.Single) {
    SceneManager.LoadScene(Path, mode);
  }

  protected override async Task LoadAsyncImpl(LoadSceneMode mode = LoadSceneMode.Single) {
    await SceneManager.LoadSceneAsync(Path, mode).ToTask();
  }

  public override async Task UnloadAsync() {
    await SceneManager.UnloadSceneAsync(Path).ToTask();
  }

}

}
