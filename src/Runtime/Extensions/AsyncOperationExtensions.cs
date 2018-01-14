using UnityEngine;
using System.Threading.Tasks;

namespace HouraiTeahouse.Loadables {

public static class AsyncOperationExtensions {

  public static Task<T> ToTask<T>(this T operation) where T : AsyncOperation {
    var task = new TaskCompletionSource<T>();
    operation.completed += op => task.SetResult(op as T);
    return task.Task;
  }

}

}
