using System;

namespace SharedLibrary
{
  [Serializable]
  public sealed class FeedItem
  {
    public Uri FeedUrl { get; set; }

    public string Title { get; set; }

    public DateTimeOffset PublishDate { get; set; }
  }
}