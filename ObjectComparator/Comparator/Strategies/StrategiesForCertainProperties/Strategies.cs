using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties
{
    /// <summary>
    ///     define a comparison strategy for certain properties
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Strategies<T> : IEnumerable<KeyValuePair<string, ICompareValues>>
    {
        private readonly IDictionary<string, ICompareValues> _strategies = new Dictionary<string, ICompareValues>();

        public IEnumerator<KeyValuePair<string, ICompareValues>> GetEnumerator()
        {
            return _strategies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Strategies<T> Set<T1>(Expression<Func<T, T1>> targetAccessor,
            Expression<Func<T1, T1, bool>> sourceValue)
        {
            return Set(targetAccessor, sourceValue, new Display());
        }

        public Strategies<T> Set<T1>(Expression<Func<T, T1>> targetAccessor,
            Expression<Func<T1, T1, bool>> sourceValue, Display distinctionRule)
        {
            var targetPropertyInfo = ToPropertyInfo(targetAccessor);
            if (_strategies.ContainsKey(targetPropertyInfo))
                throw new KeyNotFoundException($"Strategy for \"{targetPropertyInfo}\" has already contain");

            _strategies.Add(targetPropertyInfo, new MemberStrategy<T1>(sourceValue, distinctionRule));

            return this;
        }

        private static string ToPropertyInfo(LambdaExpression expression)
        {
            var exp = ReplaceValuesDictionary.Replace(expression.ToString());
            //Foo.name => name
            //all after first dot Foo.name.second => name.second
            var reg = new Regex(@"(?<=\.).*");
            var match = reg.Match(exp);
            var propName = match.Success ? match.Value : throw new Exception($"We could not parse {expression}");
            return propName;
        }
    }
}