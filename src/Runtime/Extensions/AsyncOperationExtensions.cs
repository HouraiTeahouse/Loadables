using HouraiTeahouse.Tasks;
using UnityEngine;

namespace HouraiTeahouse.Loadables {

public static class AsyncOperationExtensions {

  public static ITask<T> ToTask<T>(this T operation) where T : AsyncOperation {
    var task = new Task<T>();
    operation.completed += op => task.Resolve(op as T);
    return task;
  }

}

}
