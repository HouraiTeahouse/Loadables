using System.Collections.Generic;
using UnityEngine;

namespace HouraiTeahouse.Loadables.AssetBundles {

public class BundleManfiestMap {

  readonly Dictionary<string, BundleMetadata> _validIdentifiers;
  public AssetBundleManifest Manifest { get; }

  public BundleMetadata this[string name] => _validIdentifiers[name];
  public IEnumerable<string> BundleNames => _validIdentifiers.Keys;

  public BundleManfiestMap(AssetBundleManifest manifest) {
      Manifest = Argument.NotNull(manifest);
      _validIdentifiers = new Dictionary<string, BundleMetadata>();
      foreach (var bundle in Manifest.GetAllAssetBundles()) {
          CreateMetadata(bundle);
      }
  }

  public bool TryGetValue(string name, out BundleMetadata bundle) {
      return _validIdentifiers.TryGetValue(name, out bundle);
  }

  void AddUnique(ICollection<string> collection, string val) {
      if (!collection.Contains(val))
          collection.Add(val);
  }

  BundleMetadata CreateMetadata(string bundle) {
      var hash = Manifest.GetAssetBundleHash(bundle);
      var name= bundle.Replace("_" + hash, "");
      BundleMetadata metadata;
      if (_validIdentifiers.TryGetValue(name, out metadata))
          return metadata;
      var paths = new List<string>();
      AddUnique(paths, bundle);
      AddUnique(paths, name);
      AddUnique(paths, hash.ToString());
      var dependencies = new List<BundleMetadata>();
      foreach (var dep in Manifest.GetAllDependencies(bundle))
          dependencies.Add(CreateMetadata(dep));
      metadata = new BundleMetadata(name, hash, dependencies, paths);
      _validIdentifiers[name] = metadata;
      return metadata;
  }
}
}
