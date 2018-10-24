using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ObjectComparator.Comparator.StrategiesForCertainProperties
{
    /// <summary>
    /// define a comparison strategy for certain properties
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class StrategiesCertainProperties<T> : IEnumerable<IMemberStrategy>
    {
        private readonly List<IMemberStrategy> _strategies = new List<IMemberStrategy>();

        public StrategiesCertainProperties<T> Set<T1>(Expression<Func<T, T1>> targetAccessor,
            Expression<Func<T1, T1, bool>> sourceValue)
        {
            return Set(targetAccessor, sourceValue, new Display());
        }

        public StrategiesCertainProperties<T> Set<T1>(Expression<Func<T, T1>> targetAccessor,
            Expression<Func<T1, T1, bool>> sourceValue, Display distinctionRule)
        {
            var targetPropertyInfo = ToPropertyInfo(targetAccessor);
            var index = _strategies.FindIndex(x => x.MemberName == targetPropertyInfo);
            if (index > -1)
            {
                _strategies.RemoveAt(index);
            }

            _strategies.Add(new MemberStrategy<T1>(targetPropertyInfo, sourceValue, distinctionRule));
            return this;
        }

        public IEnumerator<IMemberStrategy> GetEnumerator()
        {
            return _strategies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static string ToPropertyInfo(LambdaExpression expression)
        {
            //Foo.name => name
            //all after first dot Foo.name.second => name.second
            var reg = new Regex(@"(?<=\.).*");
            var match = reg.Match(expression.ToString());
            var propName = match.Success ? match.Value : throw new Exception($"We could not parse {expression}");
            return propName;
        }
    }
}