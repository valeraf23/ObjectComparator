using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ObjectsComparator.Comparator.RepresentationDistinction
{
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class DeepEqualityResult : IEnumerable<Distinction>
    {
        private readonly List<Distinction> _list;

        private DeepEqualityResult()
        {
            _list = new List<Distinction>();
        }

        private DeepEqualityResult(int capacity) : this()
        {
            _list = new List<Distinction>(capacity);
        }

        private DeepEqualityResult(IEnumerable<Distinction> collection)
        {
            _list = new List<Distinction>(collection);
        }

        public Distinction this[int i]
        {
            get => _list[i];
            set => _list.Add(value);
        }

        public IEnumerator<Distinction> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static DeepEqualityResult None() => new DeepEqualityResult(0);

        public static DeepEqualityResult Create() => new DeepEqualityResult();

        public static DeepEqualityResult Create(string name, object expectedValue, object actuallyValue) =>
            Create(new Distinction(name, expectedValue, actuallyValue));

        public static DeepEqualityResult Create(IEnumerable<Distinction> collection) =>
            new DeepEqualityResult(collection);

        public static DeepEqualityResult Create(Distinction distinction) => new DeepEqualityResult(1) {distinction};

        public static ForDistinctionsBuilder<T> CreateFor<T>(string name, T expectedValue,
            T actuallyValue) where T : notnull =>
            new ForDistinctionsBuilder<T>(name, expectedValue, actuallyValue);

        public static ForDistinctionsBuilder<T> CreateFor<T>(string name, T expectedValue,
            T actuallyValue, string details) where T : notnull =>
            new ForDistinctionsBuilder<T>(name, expectedValue, actuallyValue, details);

        public DeepEqualityResult Add(Distinction input)
        {
            _list.Add(input);
            return this;
        }

        public bool IsEmpty() => _list.Any() == false;

        public bool IsNotEmpty() => !IsEmpty();

        public int Count() => _list.Count;

        public DeepEqualityResult AddRange(DeepEqualityResult collection)
        {
            _list.AddRange(collection);
            return this;
        }

        public override string ToString()
        {
            if (!_list.Any()) return "Objects are deeply equal";


            return _list.Aggregate(new StringBuilder(),
                (sb, distinction) =>
                {
                    sb.AppendLine(distinction.ToString());
                    sb.AppendLine();
                    return sb;
                },
                sb => sb.ToString()).TrimEnd();

        }

        public static implicit operator bool(DeepEqualityResult deepEqualResult) => deepEqualResult.IsEmpty();
    }

}