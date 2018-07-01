using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace HouraiTeahouse.Loadables.AssetBundles {

    public class BundleMetadata {

        public string Name { get; private set; }
        public Hash128 Hash { get; private set; }
        public ReadOnlyCollection<string> Paths { get; private set; }
        public ReadOnlyCollection<BundleMetadata> Dependencies { get; private set; }

        internal BundleMetadata(string baseName) {
          Name = baseName;
          Hash = new Hash128();
          Dependencies = new ReadOnlyCollection<BundleMetadata>(new BundleMetadata[0]);
          Paths = new ReadOnlyCollection<string>(new string[0]);
        }

        internal BundleMetadata(string baseName,
                                Hash128 hash,
                                IEnumerable<BundleMetadata> dependencies,
                                IEnumerable<string> alternativeNames) {
            Name = baseName;
            Hash = hash;
            Paths = new ReadOnlyCollection<string>(alternativeNames.ToArray());
            Dependencies = new ReadOnlyCollection<BundleMetadata>(dependencies.ToArray());
        }

    }

}
