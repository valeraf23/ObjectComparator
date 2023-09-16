using System;
using System.Collections.Concurrent;

namespace ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;

public class MemberStrategyBase
{
    protected static readonly ConcurrentDictionary<string, Delegate> Cache = new();
}