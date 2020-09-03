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

        private Distinctions()
        {
            _list = new List<Distinction>();
        }

        private Distinctions(int capacity)
        {
            _list = new List<Distinction>(capacity);
        }

        private Distinctions(IEnumerable<Distinction> collection)
        {
            _list = new List<Distinction>(collection);
        }

        public Distinction this[int i]
        {
            get => _list[i];
            set => _list.Add(value);
        }

        public IEnumerator<Distinction> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Distinctions None()
        {
            return new Distinctions(0);
        }

        public static Distinctions Create()
        {
            return new Distinctions();
        }

        public static Distinctions Create(string name, object expectedValue, object actuallyValue)
        {
            return Create(new Distinction(name, expectedValue, actuallyValue));
        }

        public static Distinctions Create(IEnumerable<Distinction> collection)
        {
            return new Distinctions(collection);
        }

        public static Distinctions Create(Distinction distinction)
        {
            return new Distinctions(1) {distinction};
        }

        public static ForDistinctionsBuilder<T> CreateFor<T>(string name, object expectedValue,
            object actuallyValue)
        {
            return new ForDistinctionsBuilder<T>(name, expectedValue, actuallyValue);
        }

        public Distinctions Add(Distinction input)
        {
            _list.Add(input);
            return this;
        }

        public bool IsEmpty()
        {
            return _list.Count == 0;
        }

        public bool IsNotEmpty()
        {
            return _list.Count > 0;
        }

        public int Count()
        {
            return _list.Count;
        }

        public Distinctions AddRange(IEnumerable<Distinction> collection)
        {
            _list.AddRange(collection);
            return this;
        }

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