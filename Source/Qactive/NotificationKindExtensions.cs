using System;
using System.Reactive;

namespace Qactive
{
  public static class NotificationKindExtensions
  {
    internal static QbservableProtocolMessageKind AsMessageKind(this NotificationKind kind)
    {
      switch (kind)
      {
        case NotificationKind.OnNext:
          return QbservableProtocolMessageKind.OnNext;
        case NotificationKind.OnCompleted:
          return QbservableProtocolMessageKind.OnCompleted;
        case NotificationKind.OnError:
          return QbservableProtocolMessageKind.OnError;
        default:
          throw new ArgumentOutOfRangeException("kind");
      }
    }
  }
}
