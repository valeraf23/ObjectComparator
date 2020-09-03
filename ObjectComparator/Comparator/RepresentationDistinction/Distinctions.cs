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
    //[DebuggerDisplay("{" + nameof(ToString) + "()}")]
    //public class Distinctions : IEnumerable
    //{
    //    private Distinction[] array = null;
    //    private int index = 0;
    //    private int capacity = 4;

    //    public Distinctions(int capacity)
    //    {
    //        this.capacity = capacity;
    //        array = new Distinction[capacity];
    //    }

    //    public Distinctions(IEnumerable<Distinction> collection)
    //    {

    //        using (var en = collection!.GetEnumerator())
    //        {
    //            while (en.MoveNext())
    //            {
    //                Add(en.Current);
    //            }
    //        }

    //    }
    //
    //     public Distinctions()
    //     {
    //         array = new Distinction[capacity];
    //     }
    //
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     public Distinctions Add(Distinction value)
    //     {
    //         if (index >= array.Length)
    //         {
    //             Expand();
    //         }
    //
    //         array[index++] = value;
    //         return this;
    //     }
    //
    //     public int Count() => array.Length;
    //
    //     public Distinctions AddRange(Distinctions collection)
    //     {
    //         for (int i = 0; i < collection.Count(); i++)
    //         {
    //             Add(collection[i]);
    //         }
    //
    //         return this;
    //     }
    //
    //     public Distinction Get(int index)
    //     {
    //         return array[index];
    //     }
    //
    //     public void Set(int index, Distinction value)
    //     {
    //         array[index] = value;
    //     }
    //
    //     public void Expand()
    //     {
    //         var newCapacity = array.Length * 2;
    //
    //         Distinction[] newArray = new Distinction[newCapacity];
    //         Array.Copy(array, newArray, array.Length);
    //         array = newArray;
    //
    //         capacity = newCapacity;
    //     }
    //
    //     public Distinction this[int index]
    //     {
    //         get { return array[index]; }
    //         set { array[index] = value; }
    //     }
    //
    //     public RefEnumerator GetEnumerator() => new RefEnumerator(array, capacity);
    //     IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
    //
    //     public struct RefEnumerator
    //     {
    //         private Distinction[] array;
    //
    //         private int index;
    //         private int capacity;
    //
    //         public RefEnumerator(Distinction[] target, int capacity)
    //         {
    //             array = target;
    //             index = -1;
    //             this.capacity = capacity;
    //         }
    //
    //         public ref Distinction Current
    //         {
    //             get
    //             {
    //                 if (array is null || index < 0 || index > capacity)
    //                 {
    //                     throw new InvalidOperationException();
    //                 }
    //
    //                 return ref array[index];
    //             }
    //         }
    //
    //         public void Dispose()
    //         {
    //         }
    //
    //         public bool MoveNext() => ++index < capacity;
    //
    //         public void Reset() => index = -1;
    //     }
    //
    //     public override string ToString()
    //     {
    //         if (!array.Any()) return "There are no Distinction";
    //
    //         var errorMessage = array.Aggregate(new StringBuilder(),
    //             (sb, distinction) => sb.AppendLine(distinction.ToString()),
    //             sb => sb.ToString());
    //
    //         return errorMessage;
    //     }
    //
    //     public static Distinctions None() => new Distinctions(0);
    //
    //     public static Distinctions Create() => new Distinctions();
    //
    //     public static Distinctions Create(string name, object expectedValue, object actuallyValue) =>
    //         Create(new Distinction(name, expectedValue, actuallyValue));
    //
    //     public static Distinctions Create(IEnumerable<Distinction> collection)
    //     {
    //         return new Distinctions(collection);
    //     }
    //
    //
    //
    //     public static Distinctions Create(Distinction distinction) => new Distinctions(1) {distinction};
    //
    //     public static ForDistinctionsBuilder<T> CreateFor<T>(string name, object expectedValue,
    //         object actuallyValue) =>
    //         new ForDistinctionsBuilder<T>(name, expectedValue, actuallyValue);
    //
    //     public bool IsEmpty() => Count() == 0;
    //     public Distinction First() => array[0];
    //     public bool IsNotEmpty() => Count() > 0;
    // }
    // }
}