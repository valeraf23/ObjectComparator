using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ObjectsComparator.Comparator.RepresentationDistinction
{
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class DistinctionsCollection : IEnumerable<Distinction>
    {
        private readonly List<Distinction> _list;

        public DistinctionsCollection() => _list = new List<Distinction>();

        public DistinctionsCollection(IEnumerable<Distinction> collection) => _list = new List<Distinction>(collection);

        public Distinction this[int i]
        {
            get => _list[i];
            set => _list.Add(value);
        }

        public static DistinctionsCollection Create(string name, object expectedValue, object actuallyValue) =>
            new DistinctionsCollection(new[] {new Distinction(name, expectedValue, actuallyValue)});

        public static DistinctionsCollection Create(IEnumerable<Distinction> collection) =>
            new DistinctionsCollection(collection);

        public static DistinctionsCollection Create(Distinction collection) =>
            new DistinctionsCollection(new[] {collection});

        public static ForDistinctionsCollectionBuilder<T> CreateFor<T>(string name, object expectedValue,
            object actuallyValue) =>
            new ForDistinctionsCollectionBuilder<T>(name, expectedValue, actuallyValue);

        public DistinctionsCollection Add(Distinction input)
        {
            _list.Add(input);
            return this;
        }

        public DistinctionsCollection AddRange(IEnumerable<Distinction> collection)
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