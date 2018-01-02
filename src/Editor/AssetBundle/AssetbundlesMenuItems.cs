using UnityEditor;

namespace HouraiTeahouse.Loadables.AssetBundles {

public class AssetBundlesMenuItems {
  const string SimulationMode = "Assets/AssetBundles/Simulation Mode";

  [MenuItem(SimulationMode)]
  public static void ToggleSimulationMode () {
    AssetBundleManager.SimulateAssetBundleInEditor = !AssetBundleManager.SimulateAssetBundleInEditor;
  }

  [MenuItem(SimulationMode, true)]
  public static bool ToggleSimulationModeValidate () {
    UnityEditor.Menu.SetChecked(SimulationMode, AssetBundleManager.SimulateAssetBundleInEditor);
    return true;
  }

}

}
