using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SharedLibrary
{
  public sealed class ChatServiceHooks
  {
    public IObservable<string> OutgoingMessages
    {
      get
      {
        return messageDispatch.AsObservable();
      }
    }

    public IObserver<string> IncomingMessages
    {
      get
      {
        return Observer.Create<string>(
          message =>
          {
            messageDispatch.OnNext(userName + " said: " + message);
          });
      }
    }

    private readonly string userName;
    private readonly ISubject<string> messageDispatch;

    public ChatServiceHooks(string userName, ISubject<string> messageDispatch)
    {
      this.userName = userName;
      this.messageDispatch = messageDispatch;
    }
  }
}