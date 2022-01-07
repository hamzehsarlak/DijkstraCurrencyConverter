using System;
using System.Collections.Generic;

namespace Algorithms.Searching.Graph
{
    public interface IVertex<T> : IComparable<IVertex<T>>, IEquatable<IVertex<T>>
    {
        public List<IVertex<T>> AdjacentVertices { get; }
        public int Weight { get; set; }
        public T Value { get; }
        public IVertex<T> PreviousVertex { get; set; }
        public object Data { get; set; }
        public void AddAdjacentVertex(IVertex<T> vertex);
    }
}
