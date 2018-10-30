﻿using System;
using System.Linq.Expressions;
using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Helpers.Extensions;

namespace ObjectComparator.Comparator.StrategiesForCertainProperties
{
    public class MemberStrategy<T> : ICompareValues
    {
        private readonly Display _display;

        public Expression<Func<T, T, bool>> CompareFunc { get; set; }

        public MemberStrategy(Expression<Func<T, T, bool>> compareFunc,
            Display display)
        {
            _display = display;
            CompareFunc = compareFunc;
        }

        public DistinctionsCollection Compare<T1>(T1 valueA, T1 valueB, string propertyName)
        {
            var a = (T) (object) valueA;
            var b = (T) (object) valueB;
            return CompareFunc.Compile()(a, b)
                ? new DistinctionsCollection()
                : new DistinctionsCollection(new[]
                {
                    _display.GetDistinction(a.ToString(), b.ToString(), propertyName,
                        BodyExpression.Get(CompareFunc).ToString())
                });
        }
    }
}