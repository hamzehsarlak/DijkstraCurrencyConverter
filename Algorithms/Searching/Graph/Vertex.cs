using System;
using System.Collections.Generic;

namespace Algorithms.Searching.Graph
{
    public class Vertex<T> : IVertex<T>
    {
        public List<IVertex<T>> AdjacentVertices { get; }
        public int Weight { get; set; }
        public T Value { get; }
        public IVertex<T> PreviousVertex { get; set; }
        public object Data { get; set; }

        public Vertex(T value, int weight = 0)
        {
            Weight = weight;
            Value = value;
            AdjacentVertices = new List<IVertex<T>>();
            PreviousVertex = null;
        }

        public Vertex(IVertex<T> vertex)
        {
            Weight = vertex.Weight;
            Value = vertex.Value;
            AdjacentVertices = vertex.AdjacentVertices;
            PreviousVertex = vertex.PreviousVertex;
        }

        public void AddAdjacentVertex(IVertex<T> vertex)
        {
            AdjacentVertices.Add(vertex);
        }


        public bool Equals(IVertex<T> other)
        {
            return other != null && Value.Equals(other.Value);
        }

        public override bool Equals(object o)
        {
            var vertex = (IVertex<T>) o;
            return vertex != null && Value.Equals(vertex.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AdjacentVertices, Weight, Value, PreviousVertex);
        }

        public int CompareTo(IVertex<T> other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Weight.CompareTo(other.Weight);
        }
    }

}