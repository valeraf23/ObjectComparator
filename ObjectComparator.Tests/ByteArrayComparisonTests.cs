using FluentAssertions;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Tests.TestModels;
using System.Linq;

namespace ObjectsComparator.Tests;

[TestFixture]
public class ByteArrayComparisonTests
{
    [Test]
    public void ByteArrayField()
    {
        var result = new BinaryPayload { Bytes = [11, 22, 33, 44] }
            .DeeplyEquals(new BinaryPayload { Bytes = [1, 2, 4, 3] });

        result.Any().Should().BeTrue();
    }

    [Test]
    public void ByteArrayFieldWithTypeStrategy()
    {
        var actual = new BinaryPayload { Bytes = [11, 22, 33, 44] };
        var expected = new BinaryPayload { Bytes = [1, 2, 4, 3] };

        var result = expected.DeeplyEquals(actual, config => config
                .WithTypeStrategies(str => str.Set<byte[]>((expected, actual) => expected.SequenceEqual(actual))));

        var distinction = result.Should().ContainSingle().Which;

        // Assertions to reflect current implementation with type strategy only.

        distinction.Path.Should().Be("BinaryPayload.Bytes");
        distinction.ExpectedValue.Should().Be("System.Byte[]");
        distinction.ActualValue.Should().Be("System.Byte[]");
        distinction.Details.Should().Be("(expected:(System.Byte[]), actual:(System.Byte[])) => expected:(System.Byte[]).SequenceEqual(actual:(System.Byte[]))");
    }

    [Test]
    public void ByteArrayFieldWithTypeStrategy_KeepOriginalValues()
    {
        var actual = new BinaryPayload { Bytes = [11, 22, 33, 44] };
        var expected = new BinaryPayload { Bytes = [1, 2, 4, 3] };

        var result = expected.DeeplyEquals(actual, config => config
            .WithTypeStrategies(str => str.Set<byte[]>(
                (expected, actual) => expected.SequenceEqual(actual),
                display => display.KeepOriginalValues())));

        var distinction = result.Should().ContainSingle().Which;

        distinction.Path.Should().Be("BinaryPayload.Bytes");
        distinction.ExpectedValue.Should().BeOfType<byte[]>();
        distinction.ActualValue.Should().BeOfType<byte[]>();
        distinction.ExpectedValue.Should().BeSameAs(expected.Bytes);
        distinction.ActualValue.Should().BeSameAs(actual.Bytes);
        distinction.Details.Should().Be("(expected:(System.Byte[]), actual:(System.Byte[])) => expected:(System.Byte[]).SequenceEqual(actual:(System.Byte[]))");
    }
}
