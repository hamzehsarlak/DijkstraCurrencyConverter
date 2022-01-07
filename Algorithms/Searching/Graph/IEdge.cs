using System;

namespace Algorithms.Searching.Graph
{
    public interface IEdge<T,TData>: IEquatable<IEdge<T,TData>>
    {
        public IVertex<T> FromVertex { get; }
        public IVertex<T> ToVertex { get; }
        public int Weight { get; }
        public TData Data { get; set; }
    }
}
