using System;

namespace ChemicalFunctions
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class AtomAttribute : Attribute
    {
        public AtomAttribute(int number, string symbol, string element, double weight)
        {
            Number = number;
            Symbol = symbol;
            Element = element;
            Weight = weight;
        }

        public int Number { get; }
        public string Symbol { get; }
        public string Element { get; }
        public double Weight { get; }
    }
}
