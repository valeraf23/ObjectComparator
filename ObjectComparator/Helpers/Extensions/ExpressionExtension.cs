using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

#nullable disable
namespace ObjectsComparator.Helpers.Extensions
{
    public static class ExpressionExtension
    {
        private static string ReplaceNull(object? value) => value == null ? "null" : $"{value}";

        public static string GetLambdaString(this LambdaExpression compareFunc, object argument,
            params object[] arguments)
        {
            var parametersName = compareFunc.Parameters.Select(c => c.Name).ToArray();

            var argList = new List<object> {argument};
            if (arguments.IsNotEmpty())
            {
                argList.AddRange(arguments);
            }

            var tLength = argList.Count;

            if (argList.Count < tLength)
            {
                throw new Exception("Provided more arguments");
            }

            if (argList.Count > tLength)
            {
                throw new Exception("Was provided more arguments than need");
            }

            var bExp = Get(compareFunc).ToString();

            var sb = new StringBuilder(bExp);
            for (var i = 0; i < parametersName.Length; i++)
            {
                sb.Replace(parametersName[i], $"{parametersName[i]}:({ReplaceNull(argList[i])})");
            }

            return sb.ToString();
        }

        public static Expression<Func<TIn, TIn, TOut>> Get<TIn, TOut>(Expression<Func<TIn, TIn, TOut>> expression)
        {
            return (Expression<Func<TIn, TIn, TOut>>) PartialEval(expression);
        }

        public static Expression Get(Expression expression)
        {
            return PartialEval(expression);
        }

        private static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        private static Expression PartialEval(Expression expression)
        {
            return PartialEval(expression, CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        private class SubtreeEvaluator : ExpressionVisitor
        {
            private readonly HashSet<Expression> _candidates;

            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {
                _candidates = candidates;
            }

            internal Expression Eval(Expression exp)
            {
                return Visit(exp);
            }

            public override Expression Visit(Expression exp)
            {
                if (exp == null) return null;

                return _candidates.Contains(exp) ? Evaluate(exp) : base.Visit(exp);
            }

            private static Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant) return e;

                var lambda = Expression.Lambda(e);
                var fn = lambda.Compile();
                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }
        }

        private class Nominator : ExpressionVisitor
        {
            private readonly Func<Expression, bool> _fnCanBeEvaluated;
            private HashSet<Expression> _candidates;
            private bool _cannotBeEvaluated;

            internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
            {
                _fnCanBeEvaluated = fnCanBeEvaluated;
            }

            internal HashSet<Expression> Nominate(Expression expression)
            {
                _candidates = new HashSet<Expression>();
                Visit(expression);
                return _candidates;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression == null) return null;
                var saveCannotBeEvaluated = _cannotBeEvaluated;

                _cannotBeEvaluated = false;

                base.Visit(expression);

                if (!_cannotBeEvaluated)
                {
                    if (_fnCanBeEvaluated(expression))
                        _candidates.Add(expression);

                    else
                        _cannotBeEvaluated = true;
                }

                _cannotBeEvaluated |= saveCannotBeEvaluated;

                return expression;
            }
        }
    }
}