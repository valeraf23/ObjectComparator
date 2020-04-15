using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;
using ObjectsComparator.Comparator.Rules.Implementations;
using ObjectsComparator.Comparator.Strategies.Implementations;
using ObjectsComparator.Comparator.Strategies.Implementations.Collections;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator
{
    public sealed class Comparator : ICompareObjectStrategy
    {
        public Comparator()
        {
            RuleForValuesTypes = new Rule<ICompareStructStrategy>(new CompareValueTypesStrategy());
            RuleForCollectionTypes = new RuleForCollections(this,
                new BaseCollectionsCompareStrategy[] {new DictionaryCompareStrategy()});
            RuleForReferenceTypes = new Rule<ICompareObjectStrategy>(this);
        }

        public Rule<ICompareObjectStrategy> RuleForReferenceTypes { get; }
        public Rule<ICompareStructStrategy> RuleForValuesTypes { get; }
        public RuleForCollections RuleForCollectionTypes { get; }

        public bool IsValid(Type member) => member.IsClass && member != typeof(string);

        public Func<string, bool> Ignore { get; set; } = p => false;
        public IDictionary<string, ICompareValues> Strategies { get; set; } = new Dictionary<string, ICompareValues>();

        public Distinctions Compare<T>(T expected, T actual, string propertyName)
        {
            if (expected != null && actual == null || expected == null && actual != null)
                return Distinctions.Create(new Distinction(typeof(T).Name, expected, actual));

            if (ReferenceEquals(expected, actual)) return Distinctions.None();
            var diff = Distinctions.Create();
            var type = expected.GetType();

            foreach (var mi in type.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(x =>
                x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field))
            {
                var name = mi.Name;
                var actualPropertyPath = MemberPathBuilder.BuildMemberPath(propertyName, mi);

                if (Ignore(actualPropertyPath)) continue;

                object firstValue = null;
                object secondValue = null;
                switch (mi.MemberType)
                {
                    case MemberTypes.Field:
                        firstValue = type.GetField(name).GetValue(expected);
                        secondValue = type.GetField(name).GetValue(actual);
                        break;
                    case MemberTypes.Property:
                        firstValue = type.GetProperty(name)?.GetValue(expected);
                        secondValue = type.GetProperty(name)?.GetValue(actual);
                        break;
                }

                var diffRes = GetDistinctions(actualPropertyPath, firstValue, secondValue);
                if (diffRes.IsNotEmpty()) diff.AddRange(diffRes);
            }

            return diff;
        }

        private Distinctions GetDifference<T>(T expected, T actual, string propertyName) =>
            RuleFactory
                .Create(RuleForCollectionTypes, RuleForReferenceTypes, RuleForValuesTypes)
                .GetFor(actual.GetType())
                .Compare(expected, actual, propertyName);

        public Distinctions Compare<T>(T expected, T actual) => Compare(expected, actual, null);

        public Distinctions GetDistinctions(string propertyName, dynamic expected, dynamic actual)
        {
            if (Strategies.IsNotEmpty() && Strategies.Any(x => x.Key == propertyName))
                return Strategies[propertyName].Compare(expected, actual, propertyName);

            if (expected == null && actual != null) return Distinctions.Create(propertyName, "null", actual);

            if (expected != null && actual == null) return Distinctions.Create(propertyName, expected, "null");

            return expected == null
                ? Distinctions.None()
                : (Distinctions) GetDifference(expected, actual, propertyName);
        }

        public void SetIgnore(Func<string, bool> ignoreStrategy)
        {
            if (ignoreStrategy == null) return;
            RuleForReferenceTypes.Strategies.ForEach(x => x.Ignore = ignoreStrategy);
        }

        public void SetStrategies(IDictionary<string, ICompareValues> strategies)
        {
            if (strategies.IsEmpty()) return;
            RuleForReferenceTypes.Strategies.ForEach(x => x.Strategies = strategies);
        }
    }
}