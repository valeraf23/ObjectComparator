using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties
{
    public sealed class Strategies<T> : IEnumerable<KeyValuePair<string, ICustomCompareValues>>
    {
        private readonly IDictionary<string, ICustomCompareValues> _strategies =
            new Dictionary<string, ICustomCompareValues>();

        public IEnumerator<KeyValuePair<string, ICustomCompareValues>> GetEnumerator() => _strategies.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Strategies<T> Set<T1>(Expression<Func<T, T1>> targetAccessor,
            Expression<Func<T1, T1, bool>> sourceValue) =>
            Set(targetAccessor, sourceValue, new Display());

        public Strategies<T> Set<T1>(Expression<Func<T, T1>> targetAccessor, Expression<Func<T1, T1, bool>> sourceValue,
            Func<Display, Display> displayBuilder)
        {
            var targetPropertyInfo = ToPropertyInfo(targetAccessor);
            if (_strategies.ContainsKey(targetPropertyInfo))
                throw new KeyNotFoundException($"Strategy for \"{targetPropertyInfo}\" has already contain");

            _strategies.Add(targetPropertyInfo, new MemberStrategy<T1>(sourceValue, displayBuilder(new Display())));

            return this;
        }

        public Strategies<T> Set<T1>(Expression<Func<T, T1>> targetAccessor, Expression<Func<T1, T1, bool>> sourceValue,
            Display distinctionRule)
        {
            var targetPropertyInfo = ToPropertyInfo(targetAccessor);
            if (_strategies.ContainsKey(targetPropertyInfo))
                throw new KeyNotFoundException($"Strategy for \"{targetPropertyInfo}\" has already contain");

            _strategies.Add(targetPropertyInfo, new MemberStrategy<T1>(sourceValue, distinctionRule));

            return this;
        }

        private static string ToPropertyInfo(LambdaExpression expression)
        {
            var parameterType = expression.Parameters.FirstOrDefault()?.Type.ToFriendlyTypeName();
            if (string.IsNullOrEmpty(parameterType))
                throw new Exception("Something went wrong. need to pass the argument (type) to retrieve property info");

            var exp = ReplaceValuesDictionary.Replace(expression.ToString());
            //Foo.name => name
            //all after first dot Foo.name.second => name.second
            var reg = new Regex(@"(?<=\.).*");
            var match = reg.Match(exp);
            var propName = match.Success ? match.Value : throw new Exception($"We could not parse {expression}");
            return $"{parameterType}.{propName}";
        }
    }
}