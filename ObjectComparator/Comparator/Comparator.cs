using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;
using ObjectsComparator.Comparator.Rules.Implementations;
using ObjectsComparator.Comparator.Strategies.Implementations;
using ObjectsComparator.Comparator.Strategies.Implementations.Collections;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator
{
    public sealed class Comparator : ICompareObjectStrategy
    {
        private static readonly MethodInfo CallGetDistinctions =
            typeof(Comparator).GetTypeInfo().GetDeclaredMethod(nameof(GetDistinctions))!;

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
        public Dictionary<string, ICompareValues> Strategies { get; set; } = new Dictionary<string, ICompareValues>();

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
                Type memberType = null;

                switch (mi.MemberType)
                {
                    case MemberTypes.Field:
                        var field = type.GetField(name);
                        firstValue = field!.GetValue(expected);
                        secondValue = field.GetValue(actual);
                        memberType = field.FieldType;
                        break;
                    case MemberTypes.Property:
                        var del = PropertyHelper.Instance(type.GetProperty(name)!);
                        firstValue = del.GetValue(expected);
                        secondValue = del.GetValue(actual);
                        memberType = del.Property.PropertyType;
                        break;
                }

                var diffRes = (Distinctions) CallGetDistinctions.MakeGenericMethod(memberType!)
                    .Invoke(this, new[] {actualPropertyPath, firstValue, secondValue});

                if (diffRes!.IsNotEmpty()) diff.AddRange(diffRes);
            }

            return diff;
        }

        private Distinctions GetDifference<T>(T expected, T actual, string propertyName) =>
            RuleFactory
                .Create(RuleForValuesTypes, RuleForCollectionTypes, RuleForReferenceTypes)
                .GetFor(actual.GetType())
                .Compare(expected, actual, propertyName);

        public Distinctions Compare<T>(T expected, T actual) => Compare(expected, actual, null);

        public void SetIgnore(Func<string, bool> ignoreStrategy)
        {
            if (ignoreStrategy == null) return;
            RuleForReferenceTypes.Strategies.ForEach(x => x.Ignore = ignoreStrategy);
        }

        public void SetStrategies(Dictionary<string, ICompareValues> strategies)
        {
            if (strategies.IsEmpty()) return;
            RuleForReferenceTypes.Strategies.ForEach(x => x.Strategies = strategies);
        }

        public Distinctions GetDistinctions<T>(string propertyName, T expected, T actual)
        {
            if (Strategies.IsNotEmpty() && Strategies.Any(x => x.Key == propertyName))
                return Strategies[propertyName].Compare(expected, actual, propertyName);

            if (expected == null && actual != null)
                return Distinctions.Create(propertyName, "null", actual);

            if (expected != null && actual == null)
                return Distinctions.Create(propertyName, expected, "null");

            if (expected == null)
                return Distinctions.None();

            var type = expected.GetType();
            if (type.IsClassAndNotString() && type.IsOverridesEqualsMethod())
            {
                var isNotEqual = !expected.Equals(actual);
                if (isNotEqual)
                    return Distinctions.Create(new Distinction(propertyName, "no info", "no info",
                        "Was used override 'Equals' method, objects not equals"));
            }

            return GetDifference(expected, actual, propertyName);
        }
    }
}