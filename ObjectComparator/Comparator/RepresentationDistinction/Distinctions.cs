using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ObjectsComparator.Comparator.RepresentationDistinction
{
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Distinctions : IEnumerable<Distinction>
    {
        private readonly List<Distinction> _list;

        public Distinctions() => _list = new List<Distinction>();

        public Distinctions(IEnumerable<Distinction> collection) => _list = new List<Distinction>(collection);

        public Distinction this[int i]
        {
            get => _list[i];
            set => _list.Add(value);
        }

        public static Distinctions Create(string name, object expectedValue, object actuallyValue) =>
            new Distinctions(new[] {new Distinction(name, expectedValue, actuallyValue)});

        public static Distinctions Create(IEnumerable<Distinction> collection) =>
            new Distinctions(collection);

        public static Distinctions Create(Distinction collection) =>
            new Distinctions(new[] {collection});

        public static ForDistinctionsCollectionBuilder<T> CreateFor<T>(string name, object expectedValue,
            object actuallyValue) =>
            new ForDistinctionsCollectionBuilder<T>(name, expectedValue, actuallyValue);

        public Distinctions Add(Distinction input)
        {
            _list.Add(input);
            return this;
        }

        public Distinctions AddRange(IEnumerable<Distinction> collection)
        {
            _list.AddRange(collection);
            return this;
        }

        public IEnumerator<Distinction> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            if (!_list.Any()) return "There are no Distinction";

            var errorMessage = _list.Aggregate(new StringBuilder(),
                (sb, distinction) => sb.AppendLine(distinction.ToString()),
                sb => sb.ToString());

            return errorMessage;
        }


    }
}