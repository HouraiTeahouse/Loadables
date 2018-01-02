using UnityEngine;

namespace HouraiTeahouse.Loadables {

public interface IResource<T> : ILoadable<T> where T : UnityEngine.Object {

  T Asset { get; }

}

}
