using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Comparator.Rules;
using ObjectComparator.Comparator.Rules.Implementations;
using ObjectComparator.Helpers.Extensions;

namespace ObjectComparator.Comparator.Implementations
{
    public sealed class CompareObjectsStrategy : ICompareObjectStrategy
    {
        public CompareObjectsStrategy()
        {
            RuleForValuesTypes = new Rule<ICompareStructStrategy>(new CompareValueTypesStrategy());
            RuleForCollectionTypes = new Rule<ICollectionsCompareStrategy>(new CollectionsCompareStrategy(this));
            RuleForReferenceTypes = new Rule<ICompareObjectStrategy>(this);
        }

        public Rule<ICompareObjectStrategy> RuleForReferenceTypes { get; }
        public Rule<ICompareStructStrategy> RuleForValuesTypes { get; }
        public Rule<ICollectionsCompareStrategy> RuleForCollectionTypes { get; }


        public bool IsValid(Type member) => member.IsClass && member != typeof(string);
        public IList<string> Ignore { get; set; } = new List<string>();
        public IDictionary<string, ICompareValues> Strategies { get; set; } = new Dictionary<string, ICompareValues>();


        public DistinctionsCollection GetDifference<T>(T valueA, T valueB, string propertyName) => RuleFactory
            .Create(RuleForCollectionTypes, RuleForReferenceTypes, RuleForValuesTypes).Get<T>()
            .Compare(valueA, valueB, propertyName);

        public DistinctionsCollection Compare<T>(T objectA, T objectB) => Compare(objectA, objectB, null);

        public DistinctionsCollection Compare<T>(T objectA, T objectB, string propertyName)
        {
            if (objectA != null && objectB == null || objectA == null && objectB != null)
            {
                return new DistinctionsCollection {new Distinction(typeof(T).Name, objectA, objectB)};
            }

            var diff = new DistinctionsCollection();
            if (ReferenceEquals(objectA, objectB)) return diff;

            var type = objectA.GetType();

            foreach (var mi in type.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(x =>
                x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field))
            {
                var name = mi.Name;
                var actualPropertyPath = MemberPathBuilder.BuildMemberPath(propertyName, mi);

                if (Ignore.Contains(actualPropertyPath))
                {
                    continue;
                }

                object valueA = null;
                object valueB = null;

                switch (mi.MemberType)
                {
                    case MemberTypes.Field:
                        valueA = type.GetField(name).GetValue(objectA);
                        valueB = type.GetField(mi.Name).GetValue(objectB);
                        break;
                    case MemberTypes.Property:
                        valueA = type.GetProperty(name).GetValue(objectA);
                        valueB = type.GetProperty(name).GetValue(objectB);
                        break;
                }

                var diffRes = CompareValuesForMember(Strategies, actualPropertyPath, valueA, valueB);
                if (diffRes.IsNotEmpty())
                {
                    diff.AddRange(diffRes);
                }
            }

            return diff;
        }

        private DistinctionsCollection CompareValuesForMember(IDictionary<string, ICompareValues> custom,
            string propertyName, dynamic valueA, dynamic valueB)
        {
            if (custom.IsEmpty() || custom.All(x => x.Key != propertyName))
            {
                var diff = new DistinctionsCollection();
                if (valueA == null && valueB != null)
                {
                    return diff.Add(new Distinction(propertyName, "null", valueB));
                }

                if (valueA != null && valueB == null)
                {
                    return diff.Add(new Distinction(propertyName, valueA, "null"));
                }

                return valueA == null
                    ? diff
                    : (DistinctionsCollection) GetDifference(valueA, valueB, propertyName);
            }

            var customStrategy = custom[propertyName];
            return customStrategy.Compare((object) valueA, (object) valueB, propertyName);
        }

        public void SetIgnore(IList<string> ignore)
        {
            if (ignore.IsEmpty()) return;
            RuleForReferenceTypes.Default.Ignore = ignore;
            RuleForReferenceTypes.Others.ForEach(x => x.Ignore = ignore);
        }

        public void SetStrategies(IDictionary<string, ICompareValues> strategies)
        {
            if (strategies.IsEmpty()) return;
            RuleForReferenceTypes.Default.Strategies = strategies;
            RuleForReferenceTypes.Others.ForEach(x => x.Strategies = strategies);
        }
    }
}