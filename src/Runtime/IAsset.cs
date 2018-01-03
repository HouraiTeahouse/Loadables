using UnityEngine;

namespace HouraiTeahouse.Loadables {

public interface IAsset<T> : ILoadable<T> where T : UnityEngine.Object {

  T Asset { get; }

}

}
