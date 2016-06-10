using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qactive.Tests.Tcp.Queries
{
  [TestClass]
  public class TcpComplexTests : TestBase
  {
    private const int leafCountPerNode = 10;

    [TestMethod]
    public async Task ComplexTest1()
    {
      var rootIds = new[] { "C", "A" };
      var rootIdsLocal = rootIds.Do(v => Debug.WriteLine("RootObject: " + v));
      var nodeIds = new[] { new[] { "2", "4" } }.ToObservable().Do(p => p.ForEach(id => Debug.WriteLine("Node: " + id)));

      var service = TcpTestService.Create(
        TcpTestService.DuplexOptions,
        new[] { typeof(RootObject), typeof(Node), typeof(Leaf), typeof(EnumerableEx) },
        Observable.Return(new ServiceContext(new[] { new RootObject("A", 1), new RootObject("B", 10), new RootObject("C", 100), new RootObject("D", 1000) })));

      var results = await service.QueryAsync(source =>
        from leafPackResult in
          from leaf in
            (from context in source
             from ids in nodeIds
             select from root in context.RootObjects.ToObservable()
                    where rootIdsLocal.Contains(root.Id)
                    from node in root[ids]
                    from leaf in node.Leaves
                    select leaf)
             .Switch()
          group leaf by leaf.Node into leavesByNode
          from leaves in leavesByNode.Buffer(leafCountPerNode * rootIds.Length)
          select new
          {
            Node = leavesByNode.Key,
            LeafPack = (from leaf in leaves
                        orderby leaf.Tick descending
                        select leaf)
                        .Distinct(leaf => leaf.Id)
                        .Memoize(memoized => new[]
                         {
                            new
                            {
                              Greens = (from leaf in memoized
                                        where leaf.IsGreen
                                        orderby leaf.Size ascending
                                        select leaf)
                                        .ToList()
                                        .AsReadOnly(),
                              Browns = (from leaf in memoized
                                        where !leaf.IsGreen
                                        orderby leaf.Size descending
                                        select leaf)
                                        .ToList()
                                        .AsReadOnly()
                            }
                         })
                        .First()
          }
        select new
        {
          Node = leafPackResult.Node,
          SmallestGreen = leafPackResult.LeafPack.Greens.FirstOrDefault(),
          LargestBrown = leafPackResult.LeafPack.Browns.FirstOrDefault()

        });

      AssertEqual(results,
        OnNext(new { Node = new Node("2"), SmallestGreen = new Leaf("2", 6), LargestBrown = new Leaf("5", 9) }),
        OnNext(new { Node = new Node("4"), SmallestGreen = new Leaf("2", 6), LargestBrown = new Leaf("5", 9) }),
        OnCompleted(new { Node = default(Node), SmallestGreen = default(Leaf), LargestBrown = default(Leaf) }));
    }

    private sealed class ServiceContext
    {
      public ServiceContext(IEnumerable<RootObject> roots)
      {
        RootObjects = roots.ToList().AsReadOnly();
      }

      public IReadOnlyCollection<RootObject> RootObjects { get; }

      public override string ToString()
        => string.Join(",", RootObjects);
    }

    [Serializable]
    private sealed class RootObjectClient : IDisposable
    {
      private readonly RootObject root;

      public RootObjectClient(RootObject root)
      {
        this.root = root;
      }

      public IObservable<Node> Nodes(IEnumerable<string> ids)
      {
        var nodes = new[] { new Node("1", root, GetLeaves), new Node("2", root, GetLeaves), new Node("3", root, GetLeaves), new Node("4", root, GetLeaves), new Node("5", root, GetLeaves) };

        return from node in nodes.ToObservable()
               where ids.Contains(node.Id)
               select node;
      }

      private IObservable<Leaf> GetLeaves(Node node)
      {
        var ids = new[] { "1", "2", "3", "4", "5" };

        return from tick in Observable.Range(0, leafCountPerNode, Scheduler.Immediate)
               select new Leaf(node, tick, ids[tick % ids.Length], tick % 2 == 0, tick * node.Root.Multiplier);
      }

      public void Dispose()
      {
      }
    }

    [Serializable]
    private sealed class RootObject
    {
      public RootObject(string id, decimal multiplier)
      {
        Id = id;
        Multiplier = multiplier;
      }

      public string Id { get; }

      public decimal Multiplier { get; }

      public IObservable<Node> this[IEnumerable<string> nodeIds]
        => Observable.Using(() => new RootObjectClient(this), client => client.Nodes(nodeIds));

      public override bool Equals(object obj) => obj is RootObject && ((RootObject)obj).Id == Id;

      public override int GetHashCode() => Id?.GetHashCode() ?? 0;

      public override string ToString() => Id;
    }

    [Serializable]
    private sealed class Node
    {
      private readonly IObservable<Leaf> leaves;

      public Node(string id)
        : this(id, null, _ => Observable.Empty<Leaf>())
      {
      }

      public Node(string id, RootObject root, Func<Node, IObservable<Leaf>> leaves)
      {
        Id = id;
        Root = root;
        this.leaves = leaves(this);
      }

      public string Id { get; }

      public RootObject Root { get; }

      public IObservable<Leaf> Leaves => leaves;

      public override bool Equals(object obj) => obj is Node && ((Node)obj).Id == Id;

      public override int GetHashCode() => Id?.GetHashCode() ?? 0;

      public override string ToString() => Id;
    }

    [Serializable]
    private sealed class Leaf
    {
      public Leaf(string id, decimal size)
      {
        Id = id;
        Size = size;
      }

      public Leaf(Node node, long tick, string id, bool isGreen, decimal size)
      {
        Node = node;
        Tick = tick;
        Id = id;
        IsGreen = isGreen;
        Size = size;
      }

      public Node Node { get; }
      public long Tick { get; }
      public string Id { get; }
      public bool IsGreen { get; }
      public decimal Size { get; }

      public override bool Equals(object obj) => obj is Leaf
                                              && ((Leaf)obj).Id == Id
                                              && ((Leaf)obj).Size == Size;

      public override int GetHashCode() => (Id?.GetHashCode() ?? 0) ^ Size.GetHashCode();

      public override string ToString() => $"{Id}:{Size} {(IsGreen ? "Green" : "Brown")}";
    }
  }
}
