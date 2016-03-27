using System.Reactive.Linq;

namespace Qactive
{
  public interface ISecureQbservable<out T> : IQbservable<T>
  {
    void PrepareUnsafe();
  }
}
