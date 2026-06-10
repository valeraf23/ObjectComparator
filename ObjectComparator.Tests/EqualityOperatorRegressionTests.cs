using FluentAssertions;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;

namespace ObjectsComparator.Tests;

[TestFixture]
public class EqualityOperatorRegressionTests
{
    private class MoneyBase
    {
        public decimal Amount { get; init; }

        public static bool operator ==(MoneyBase left, MoneyBase right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            return left is not null && right is not null && left.Amount == right.Amount;
        }

        public static bool operator !=(MoneyBase left, MoneyBase right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is MoneyBase other && this == other;
        }

        public override int GetHashCode()
        {
            return Amount.GetHashCode();
        }
    }

    private class DerivedMoney : MoneyBase;

    private class Wallet
    {
        public MoneyBase Balance { get; init; }
    }

    [Test]
    public void Member_With_Derived_Runtime_Type_Uses_Inherited_Equality_Operator()
    {
        // The member is declared as MoneyBase (which defines op_Equality) but holds
        // a derived runtime type. Previously the operator cache was populated under the
        // declared type while Compare looked up the runtime type, so equal values were
        // reported as different.
        var expected = new Wallet { Balance = new DerivedMoney { Amount = 10m } };
        var actual = new Wallet { Balance = new DerivedMoney { Amount = 10m } };

        var result = expected.DeeplyEquals(actual);

        result.IsEmpty().Should().BeTrue(result.ToString());
    }

    [Test]
    public void Member_With_Derived_Runtime_Type_Detects_Inequality_Via_Operator()
    {
        var expected = new Wallet { Balance = new DerivedMoney { Amount = 10m } };
        var actual = new Wallet { Balance = new DerivedMoney { Amount = 11m } };

        var result = expected.DeeplyEquals(actual);

        result.IsNotEmpty().Should().BeTrue();
    }

    [Test]
    public void Same_Reference_Is_Deeply_Equal()
    {
        var wallet = new Wallet { Balance = new DerivedMoney { Amount = 10m } };

        var result = wallet.DeeplyEquals(wallet);

        result.IsEmpty().Should().BeTrue();
    }
}
