using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HouraiTeahouse.Loadables {

/// <summary>
/// Custom PropertyDrawer for ResourcePathAttribute.
/// </summary>
[CustomPropertyDrawer(typeof(ResourceAttribute))]
internal class ResourceAttributeDrawer : PropertyDrawer {

  class Data {

    Object _object;
    string _path;
    public readonly GUIContent Content;

    public Data(SerializedProperty property, GUIContent content) {
      _path = property.stringValue;
      if (Resource.IsBundlePath(_path)) {
        var bundlePath = Resource.SplitBundlePath(_path);
        _object = AssetUtil.LoadBundledAsset<Object>(bundlePath.Item1, bundlePath.Item2);
      } else {
        _object = Resources.Load<Object>(_path);
      }
      Content = new GUIContent(content);
      UpdateContent(content);
    }

    bool Valid =>  !string.IsNullOrEmpty(_path);

    public void Draw(Rect position, SerializedProperty property, Type type) {
      EditorGUI.BeginChangeCheck();
      var oldColor = GUI.color;
      GUI.color = Valid ? GUI.color : Color.red;
      Object obj = EditorGUI.ObjectField(position, Content, _object, type, false);
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
      } else if (!Valid) {
        message = "Not in Resources folder or Asset Bundle. Will not be saved.";
      } else if (_path.IndexOf(Resource.BundleSeperator) >= 0) {
        var bundlePath = Resource.SplitBundlePath(_path);
        message = $"Asset Bundle: {bundlePath.Item1}\nPath:{bundlePath.Item2}";
      } else {
        message = $"Path: {_path}";
      }

      if (string.IsNullOrEmpty(label.tooltip)) {
        Content.tooltip = message;
      } else {
        Content.tooltip = $"{label.tooltip}\n{message}";
      }
    }

    void Update(Object obj) {
      _object = obj;
      var bundleName = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_object)).assetBundleName;
      if (AssetUtil.IsResource(_object)) {
        _path = AssetUtil.GetResourcePath(_object);
      } else if (!string.IsNullOrEmpty(bundleName)) {
        _path = bundleName + Resource.BundleSeperator + _object.name;
      } else {
        _path = string.Empty;
      }
    }

  }

  readonly Dictionary<string, Data> _data;

  public ResourceAttributeDrawer() {
    _data = new Dictionary<string, Data>();
  }

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
    data.Draw(position, property, (attribute as ResourceAttribute).TypeRestriction);
    _data[propertyPath] = data;
    EditorGUI.EndProperty();
  }

}

}
