using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Object = UnityEngine.Object;

namespace HouraiTeahouse.Loadables {

/// <summary>
/// Custom PropertyDrawer for SceneAttribute.
/// </summary>
[CustomPropertyDrawer(typeof(SceneAttribute))]
internal class SceneAttributeDrawer : PropertyDrawer {

  readonly Dictionary<string, Data> _data;

  public SceneAttributeDrawer() {
    _data = new Dictionary<string, Data>();
  }

  class Data {

    SceneAsset _object;
    string _path;
    public readonly GUIContent Content;

    public Data(SerializedProperty property, GUIContent content) {
      _path = property.stringValue;
      string path = $"Assets/{_path}.unity";
      if (Bundles.IsBundlePath(_path)) {
        var bundlePath = Bundles.SplitBundlePath(_path);
        var paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundlePath.Item1, bundlePath.Item2);
        path = paths.FirstOrDefault() ?? path;
      }
      _object = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
      Content = new GUIContent(content);
      UpdateContent(content);
    }

    bool IsValid =>  !string.IsNullOrEmpty(_path);

    public void Draw(Rect position, SerializedProperty property) {
      EditorGUI.BeginChangeCheck();
      SceneAsset obj;

      var oldColor = GUI.color;
      GUI.color = IsValid ? GUI.color : Color.red;
      obj = EditorGUI.ObjectField(position, Content, _object, typeof(SceneAsset), false) as SceneAsset;
      GUI.color = oldColor;

      if (!EditorGUI.EndChangeCheck()) {
        return;
      }
      Update(obj);
      property.stringValue = _path;
      EditorUtility.SetDirty(property.serializedObject.targetObject);
    }

    public void UpdateContent(GUIContent label) {
      string message;
      if (!_object) {
        message = "No object specified";
      } else if (!IsValid) {
        message = "Not in Build Settings or Asset Bundle. Will not be saved.";
      } else if (Bundles.IsBundlePath(_path)) {
        var bundlePath = Bundles.SplitBundlePath(_path);
        message = $"Asset Bundle: {bundlePath.Item1}\nPath:{bundlePath.Item2}";
      } else {
        message = string.Format("Path: {0}", _path);
      }

      Content.tooltip = string.IsNullOrEmpty(label.tooltip) ? message : string.Format("{0}\n{1}", label.tooltip, message);
    }

    void Update(SceneAsset obj) {
      _object = obj;
      var scenePath = GetScenePath(obj);
      var bundleName = GetBundlePath(obj);
      _path = scenePath ?? bundleName ?? string.Empty;
    }

    string GetScenePath(SceneAsset scene) {
      var assetPath = AssetDatabase.GetAssetPath(scene);
      var scenePath = Regex.Replace(assetPath, "Assets/(.*)\\..*", "$1");
      return EditorBuildSettings.scenes.Select(s => s.path)
                                       .FirstOrDefault(p => p == scenePath);
    }

    string GetBundlePath(Object obj) {
      var assetPath = AssetDatabase.GetAssetPath(obj);
      var bundleName = AssetImporter.GetAtPath(assetPath).assetBundleName;
      if (!string.IsNullOrEmpty(bundleName)) {
        return Bundles.CreateBundlePath(bundleName, assetPath);
      } else {
        return null;
      }
    }

  }

  /// <summary>
  ///     <see cref="PropertyDrawer.OnGUI" />
  /// </summary>
  public override void OnGUI(Rect position,
                             SerializedProperty property,
                             GUIContent label) {
    if (property.propertyType != SerializedPropertyType.String) {
      EditorGUI.PropertyField(position, property, label);
      return;
    }

    string propertyPath = property.propertyPath;
    Data data;
    if (!_data.TryGetValue(propertyPath, out data)) {
      data = new Data(property, label);
      _data[propertyPath] = data;
    }

    EditorGUI.BeginProperty(position, data.Content, property);
    data.UpdateContent(label);
    data.Draw(position, property);
    _data[propertyPath] = data;
    EditorGUI.EndProperty();
  }

}

}
