using System;
using System.Collections.Generic;
using System.Linq;
using Algorithms.Searching.Graph;

namespace Algorithms
{
    public sealed class CurrencyConverter : ICurrencyConverter
    {
        private static readonly Lazy<CurrencyConverter>
            Lazy = new(() => new CurrencyConverter());

        private IEnumerable<Tuple<string, string, double>> _conversionRates;
        private IGraph<string,EdgeData> _graph;

        public static CurrencyConverter Instance => Lazy.Value;

        private CurrencyConverter()
        {
        }

        public void ClearConfiguration()
        {
            _conversionRates = null;
            _graph = null;
        }

        public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
        {
            if (conversionRates == null || !conversionRates.Any())
                throw new Exception("conversionRates must have value");
            _conversionRates = conversionRates;
            InitData();
        }

        public double Convert(string fromCurrency, string toCurrency, double amount)
        {
            if (_conversionRates == null)
                throw new Exception("Configuration are not initialized");
            if (fromCurrency == toCurrency) return amount;
            var result = ConvertFromPath(fromCurrency, toCurrency, amount);
            if (result >= 0) return result;
            return ConvertFromReversePath(fromCurrency, toCurrency, amount);
        }

        private double ConvertFromPath(string fromCurrency, string toCurrency, double amount)
        {
            var path = _graph.FindShortestPath(fromCurrency, toCurrency)[0];
            if (!path.Any()) return -1;
            var result = amount;
            if (path.Last().FromVertex.Value != fromCurrency) return -1;
            for (var i = path.Count - 1; i >= 0; i--)
            {
                var c = path[i];
                result *= c.Data.ConversionRate;
            }
            return result;
        }

        private double ConvertFromReversePath(string fromCurrency, string toCurrency, double amount)
        {
            var path = _graph.FindShortestPath(toCurrency, fromCurrency)[0];
            if (!path.Any()) return -1;
            var result = amount;
            if (path.Last().FromVertex.Value != toCurrency) return -1;
            for (var i = path.Count - 1; i >= 0; i--)
            {
                var c = path[i];
                result /= c.Data.ConversionRate;
            }
            return result;
        }

        private void InitData()
        {
            _graph = new Graph<string,EdgeData>();

            foreach (var (item1, item2, item3) in _conversionRates)
            {
                _graph.AddEdge(item1, item2, 1, new EdgeData { ConversionRate = item3 });
                _graph.AddEdge(item2, item1, 1, new EdgeData { ConversionRate = 1/item3 });
            }
        }
    }

    public class EdgeData
    {
        public double ConversionRate { get; set; }
    }
}
