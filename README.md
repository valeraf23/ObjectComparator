# ObjectComparator

[![NuGet.org](https://img.shields.io/nuget/v/ObjectComparator.svg?style=flat-square&label=NuGet.org)](https://www.nuget.org/packages/ObjectComparator/)
![Nuget](https://img.shields.io/nuget/dt/ObjectComparator)
[![Build status](https://ci.appveyor.com/api/projects/status/1i6lq6mft1jy94vx/branch/master?svg=true)](https://ci.appveyor.com/project/valeraf23/objectcomparator/branch/master)
[![.NET Actions Status](https://github.com/valeraf23/ObjectComparator/workflows/.NET/badge.svg)](https://github.com/valeraf23/ObjectComparator/actions)

ObjectComparator is a high-performance .NET library designed for deep comparison of complex objects. The library not only identifies differences, it also highlights the exact properties and values that diverge. Developers can easily configure custom comparison rules, ignore members, and fine tune the comparison pipeline to match real-world scenarios.

## Table of Contents
- [Key Features](#key-features)
- [Installation](#installation)
- [Getting Started](#getting-started)
- [Usage Examples](#usage-examples)
  - [Basic Comparison](#basic-comparison)
  - [Unified Fluent Configuration (Recommended)](#unified-fluent-configuration-recommended)
  - [Type-Based Comparison Strategies](#type-based-comparison-strategies)
  - [Custom Strategies for Comparison](#custom-strategies-for-comparison)
  - [Collection Comparison with Custom Strategies](#collection-comparison-with-custom-strategies)
  - [Ignoring Specific Properties or Fields](#ignoring-specific-properties-or-fields)
  - [Display Distinctions with Custom Strategy](#display-distinctions-with-custom-strategy)
  - [Comparison for Collection Types](#comparison-for-collection-types)
  - [Comparison for Dictionary Types](#comparison-for-dictionary-types)
  - [Ignore Strategy](#ignore-strategy)
  - [DeeplyEquals when Equals Is Overridden](#deeplyequals-when-equals-is-overridden)
  - [DeeplyEquals when Equality Operator Is Overridden](#deeplyequals-when-equality-operator-is-overridden)
  - [Display Distinctions for Dictionary Types](#display-distinctions-for-dictionary-types)
  - [Comparison for Anonymous Types](#comparison-for-anonymous-types)
  - [Comparison for Different Types](#comparison-for-different-types)
  - [Convert Comparison Result to JSON](#convert-comparison-result-to-json)
  - [Configuring the Comparison Pipeline](#configuring-the-comparison-pipeline)
- [Working with the Source](#working-with-the-source)
- [Contributing](#contributing)
- [License](#license)

## Key Features

- **Deep member-by-member comparisons:** Unravel every detail and identify even the slightest differences between complex objects.
- **Customizable rules:** Define bespoke comparison criteria for properties or fields so that you remain in control of the comparison process.
- **Type-based strategies:** Apply custom comparison logic to all properties of a specific type (e.g., case-insensitive string comparison everywhere).
- **Unified fluent API:** Configure all comparison options in one place using a clean, chainable builder pattern.
- **Collection support:** Compare collections with custom strategies that apply to all elements automatically.
- **Performance focused:** Despite its comprehensive comparisons, ObjectComparator is optimized for speed and minimal allocations.
- **Friendly diagnostics:** Differences are captured with paths, expected values, actual values, and optional details, making debugging straightforward.

## Installation

### NuGet Package Manager Console
```bash
Install-Package ObjectComparator
```

#### Install with .NET CLI
```
dotnet add package ObjectComparator
```

## Getting Started

ObjectComparator targets modern .NET versions (netstandard2.1 and higher). Install the NuGet package and add `using ObjectComparator;` to access the extension methods. The library works seamlessly in unit tests, integration tests, and production services.

```csharp
using ObjectsComparator;

var result = actual.DeeplyEquals(expected);
```

The returned `DeepEqualityResult` contains one entry per difference. When there are no differences, `result.IsEmpty` is `true` and the compared objects are considered deeply equal.

## Usage Examples

### Basic Comparison

Compare two `Student` objects and identify the differences.

```csharp
var actual = new Student
{
    Name = "Alex",
    Age = 20,
    Vehicle = new Vehicle
    {
        Model = "Audi"
    },
    Courses = new[]
    {
        new Course
        {
            Name = "Math",
            Duration = TimeSpan.FromHours(4)
        },
        new Course
        {
            Name = "Liter",
            Duration = TimeSpan.FromHours(4)
        }
    }
};

var expected = new Student
{
    Name = "Bob",
    Age = 20,
    Vehicle = new Vehicle
    {
        Model = "Opel"
    },
    Courses = new[]
    {
        new Course
        {
            Name = "Math",
            Duration = TimeSpan.FromHours(3)
        },
        new Course
        {
            Name = "Literature",
            Duration = TimeSpan.FromHours(4)
        }
    }
};

var result = actual.DeeplyEquals(expected);

/*
    Path: "Student.Name":
    Expected Value :Alex
    Actually Value :Bob

    Path: "Student.Vehicle.Model":
    Expected Value :Audi
    Actually Value :Opel

    Path: "Student.Courses[0].Duration":
    Expected Value :04:00:00
    Actually Value :03:00:00

    Path: "Student.Courses[1].Name":
    Expected Value :Liter
    Actually Value :Literature
*/
```

### Unified Fluent Configuration (Recommended)

The unified fluent API allows you to configure all comparison options in one place using a clean, chainable builder pattern. This is the recommended approach for complex comparison scenarios.

```csharp
var expected = new VehicleDto
{
    Id = 1,
    Model = "",
    Description = "Test",
    InternalCode = "ABC"
};

var actual = new VehicleEntity
{
    Id = 1,
    Model = null,
    Description = "Test",
    InternalCode = "XYZ"  // Different but will be ignored
};

// Combine all configuration in one fluent call
var result = expected.DeeplyEquals(actual, config => config
    .AllowDifferentTypes()
    .Ignore("InternalCode")
    .WithTypeStrategies(ts => ts.Set<string>((e, a) =>
        (string.IsNullOrEmpty(e) && string.IsNullOrEmpty(a)) || e == a))
    .WithStrategies(s => s.Set(x => x.Description, (e, a) => e == a)));

// Objects are deeply equal - null and empty strings treated as equal,
// InternalCode ignored, and different types allowed
```

**Available configuration options:**

| Method | Description |
|--------|-------------|
| `.AllowDifferentTypes()` | Enable comparing objects of different types by matching property names |
| `.Ignore("Prop1", "Prop2")` | Ignore specific properties by name |
| `.IgnoreWhen(path => path.EndsWith("Id"))` | Ignore properties matching a predicate |
| `.Skip(StrategyType.OverridesEquals)` | Skip specific comparison strategies |
| `.WithTypeStrategies(...)` | Apply custom comparison to all properties of a type |
| `.WithStrategies(...)` | Apply custom comparison to specific properties |

### Type-Based Comparison Strategies

Apply custom comparison logic to all properties of a specific type. This is useful for scenarios like case-insensitive string comparison or treating null and empty strings as equal across all string properties.

```csharp
var expected = new Person
{
    FirstName = "JOHN",
    LastName = "DOE",
    Email = ""
};

var actual = new Person
{
    FirstName = "john",
    LastName = "Doe",
    Email = null
};

// Case-insensitive comparison for all strings, treating null/empty as equal
var result = expected.DeeplyEquals(actual, config => config
    .WithTypeStrategies(ts => ts.Set<string>((e, a) =>
        (string.IsNullOrEmpty(e) && string.IsNullOrEmpty(a)) ||
        string.Equals(e, a, StringComparison.OrdinalIgnoreCase))));

// Objects are deeply equal
```

**Multiple type strategies:**

```csharp
var result = expected.DeeplyEquals(actual, config => config
    .WithTypeStrategies(ts => ts
        .Set<string>((e, a) => string.Equals(e, a, StringComparison.OrdinalIgnoreCase))
        .Set<DateTime>((e, a) => e.Date == a.Date)  // Compare dates only, ignore time
        .Set<decimal>((e, a) => Math.Abs(e - a) < 0.01m)));  // Tolerance for decimals
```

**Using runtime Type:**

```csharp
var result = expected.DeeplyEquals(actual, config => config
    .WithTypeStrategies(ts => ts.Set(typeof(string), (e, a) =>
        string.Equals((string?)e, (string?)a, StringComparison.OrdinalIgnoreCase))));
```

### Custom Strategies for Comparison

Define specific strategies for comparing properties.

```csharp
var result = actual.DeeplyEquals(
    expected,
    strategy => strategy
        .Set(x => x.Vehicle.Model, (act, exp) => act.Length == exp.Length)
        .Set(x => x.Courses[1].Name, (act, exp) => act.StartsWith('L') && exp.StartsWith('L')));

/*
    Path: "Student.Name":
    Expected Value :Alex
    Actually Value :Bob

    Path: "Student.Courses[0].Duration":
    Expected Value :04:00:00
    Actually Value :03:00:00
*/
```

### Collection Comparison with Custom Strategies

When comparing collections, you can apply custom strategies that automatically apply to all elements. This is useful for comparing lists of DTOs with entities or applying consistent comparison rules across collection items.

```csharp
var expected = new List<VehicleDto>
{
    new() { Id = 1, Model = "BMW", Description = "", InternalCode = "A" },
    new() { Id = 2, Model = "Audi", Description = null, InternalCode = "B" },
    new() { Id = 3, Model = null, Description = "Description", InternalCode = "C" }
};

var actual = new List<VehicleDto>
{
    new() { Id = 1, Model = "BMW", Description = null, InternalCode = "A" },
    new() { Id = 2, Model = "Audi", Description = "", InternalCode = "B" },
    new() { Id = 3, Model = "", Description = "Description", InternalCode = "C" }
};

// Custom strategy applies to all collection elements
var result = expected.DeeplyEquals(actual,
    strategy => strategy
        .Set(x => x.Model, (exp, act) =>
            (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act)
        .Set(x => x.Description, (exp, act) =>
            (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act));

// Objects are deeply equal - null and empty strings treated as equal for Model and Description
```

**Comparing collections of different types:**

```csharp
var expected = new List<VehicleDto>
{
    new() { Id = 1, Model = "", Description = "Test", InternalCode = "ABC" },
    new() { Id = 2, Model = null, Description = "Test", InternalCode = "DEF" }
};

var actual = new List<VehicleEntity>
{
    new() { Id = 1, Model = null, Description = "Test", InternalCode = "XYZ" },
    new() { Id = 2, Model = "", Description = "Test", InternalCode = "XYZ" }
};

// Compare different types with custom strategy and ignore
var result = expected.DeeplyEquals(actual,
    strategy => strategy
        .Set(x => x.Model, (exp, act) =>
            (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act),
    options => options.AllowDifferentTypes(),
    "InternalCode");

// Objects are deeply equal
```

### Ignoring Specific Properties or Fields

Omit certain properties or fields from the comparison.

```csharp
var ignore = new[] { "Name", "Courses", "Vehicle" };
var result = actual.DeeplyEquals(expected, ignore);

/*
    Objects are deeply equal
*/
```

### Display Distinctions with Custom Strategy

Provide specific strategies and display the differences.

```csharp
var result = actual.DeeplyEquals(
    expected,
    strategy => strategy
        .Set(x => x.Vehicle.Model, (act, exp) => act.StartsWith('A') && exp.StartsWith('A')),
    "Name",
    "Courses");

/*
    Path: "Student.Vehicle.Model":
    Expected Value :Audi
    Actually Value :Opel
    Details : (act:(Audi), exp:(Opel)) => (act:(Audi).StartsWith(A) AndAlso exp:(Opel).StartsWith(A))
*/

var skip = new[] { "Vehicle", "Name", "Courses[1].Name" };
var resultWithDisplay = expected.DeeplyEquals(
    actual,
    str => str.Set(
        x => x.Courses[0].Duration,
        (act, exp) => act > TimeSpan.FromHours(3),
        new Display { Expected = "Expected that Duration should be more that 3 hours" }),
    skip);

/*
    Path: "Student.Courses[0].Duration":
    Expected Value :Expected that Duration should be more that 3 hours
    Actually Value :04:00:00
    Details : (act:(03:00:00), exp:(04:00:00)) => (act:(03:00:00) > 03:00:00)
*/
```

### Comparison for Collection Types

Identify differences between two list or array-based collection objects, including nested structures.

```csharp
var actual = new GroupPortals
{
    Portals = new List<int> { 1, 2, 3, 5 },
    Portals1 = new List<GroupPortals1>
    {
        new GroupPortals1
        {
            Courses = new List<Course>
            {
                new Course { Name = "test" }
            }
        }
    }
};

var expected = new GroupPortals
{
    Portals = new List<int> { 1, 2, 3, 4, 7, 0 },
    Portals1 = new List<GroupPortals1>
    {
        new GroupPortals1
        {
            Courses = new List<Course>
            {
                new Course { Name = "test1" }
            }
        }
    }
};

var result = expected.DeeplyEquals(actual);

/*
    Path: "GroupPortals.Portals[3]":
    Expected Value: 4
    Actual Value: 5

    Path: "GroupPortals.Portals[4]":
    Expected Value: 7
    Actual Value:
    Details: Removed

    Path: "GroupPortals.Portals[5]":
    Expected Value: 0
    Actual Value:
    Details: Removed

    Path: "GroupPortals.Portals1[0].Courses[0].Name":
    Expected Value: test1
    Actual Value: test
*/
```

### Comparison for Dictionary Types

Identify differences between two dictionary objects.

```csharp
var expected = new Library
{
    Books = new Dictionary<string, Book>
    {
        ["hobbit"] = new Book { Pages = 1000, Text = "hobbit Text" },
        ["murder in orient express"] = new Book { Pages = 500, Text = "murder in orient express Text" },
        ["Shantaram"] = new Book { Pages = 500, Text = "Shantaram Text" }
    }
};

var actual = new Library
{
    Books = new Dictionary<string, Book>
    {
        ["hobbit"] = new Book { Pages = 1, Text = "hobbit Text" },
        ["murder in orient express"] = new Book { Pages = 500, Text = "murder in orient express Text1" },
        ["Shantaram"] = new Book { Pages = 500, Text = "Shantaram Text" },
        ["Shantaram1"] = new() { Pages = 500, Text = "Shantaram Text" }
    }
};

var result = expected.DeeplyEquals(actual);

/*
    Path: "Library.Books":
    Expected Value:
    Actual Value: Shantaram1
    Details: Added

    Path: "Library.Books[hobbit].Pages":
    Expected Value: 1000
    Actual Value: 1

    Path: "Library.Books[murder in orient express].Text":
    Expected Value: murder in orient express Text
    Actual Value: murder in orient express Text1
*/
```

### Ignore Strategy

Apply a strategy to ignore certain comparisons based on conditions.

```csharp
var act = new Student
{
    Name = "StudentName",
    Age = 1,
    Courses = new[]
    {
        new Course
        {
            Name = "CourseName"
        }
    }
};

var exp = new Student
{
    Name = "StudentName1",
    Age = 1,
    Courses = new[]
    {
        new Course
        {
            Name = "CourseName1"
        }
    }
};

var distinctions = act.DeeplyEquals(exp, propName => propName.EndsWith("Name"));
/*
    Objects are deeply equal
*/
```

### DeeplyEquals when Equals Is Overridden

```csharp
var actual = new SomeTest("A");
var expected = new SomeTest("B");

var result = exp.DeeplyEquals(act);

/*
    Path: "SomeTest":
    Expected Value :ObjectsComparator.Tests.SomeTest
    Actually Value :ObjectsComparator.Tests.SomeTest
    Details : Was used override 'Equals()'
*/
```

### DeeplyEquals when Equality Operator Is Overridden

```csharp
/*
    Path: "SomeTest":
    Expected Value :ObjectsComparator.Tests.SomeTest
    Actually Value :ObjectsComparator.Tests.SomeTest
    Details : == (Equality Operator)
*/
```

### Display Distinctions for Dictionary Types

```csharp
var firstDictionary = new Dictionary<string, string>
{
    { "Key", "Value" },
    { "AnotherKey", "Value" },
};

var secondDictionary = new Dictionary<string, string>
{
    { "Key", "Value" },
    { "AnotherKey", "AnotherValue" },
};

var result = firstDictionary.DeeplyEquals(secondDictionary);

/*
    Path: "Dictionary<String, String>[AnotherKey]":
    Expected Value :Value
    Actually Value :AnotherValue
*/
```

### Comparison for Anonymous Types

Detect differences when dealing with anonymous types.

```csharp
var actual = new { Integer = 1, String = "Test", Nested = new byte[] { 1, 2, 3 } };
var expected = new { Integer = 1, String = "Test", Nested = new byte[] { 1, 2, 4 } };

var result = exp.DeeplyEquals(act);

/*
    Path: "AnonymousType<Int32, String, Byte[]>.Nested[2]":
    Expected Value :3
    Actually Value :4
*/
```

### Comparison for Different Types

Compare objects with different types that share the same shape or property names. This is useful when comparing DTOs to entities or migrating between different model versions.

```csharp
var expected = new StudentDto
{
    Name = "Alex",
    Age = 20
};

var actual = new StudentEntity
{
    Name = "Alex",
    Age = 21
};

// Using unified configuration (recommended)
var result = expected.DeeplyEquals(actual, config => config.AllowDifferentTypes());

/*
    Path: "StudentDto.Age":
    Expected Value: 20
    Actual Value: 21
*/
```

**Combining different types with custom strategies:**

```csharp
var expected = new VehicleDto
{
    Id = 1,
    Model = "",
    Description = null,
    InternalCode = "CODE"
};

var actual = new VehicleEntity
{
    Id = 1,
    Model = null,
    Description = "",
    InternalCode = "CODE"
};

// Different types + type strategies + ignore in one call
var result = expected.DeeplyEquals(actual, config => config
    .AllowDifferentTypes()
    .Ignore("Id")
    .WithTypeStrategies(ts => ts.Set<string>((e, a) =>
        (string.IsNullOrEmpty(e) && string.IsNullOrEmpty(a)) ||
        string.Equals(e, a, StringComparison.OrdinalIgnoreCase))));

// Objects are deeply equal
```

### Convert Comparison Result to JSON

You can serialize the result of object comparison (DeepEqualityResult) into a structured JSON format, suitable for logging, UI display, or audits.

```csharp
var distinctions = DeepEqualityResult.Create(new[]
{
    new Distinction("Snapshot.Status", "Active", "Deprecated", "Different state"),
    new Distinction("Snapshot.Rules[2].Expression", "Amount > 100", "Amount > 200"),
    new Distinction("Snapshot.Rules[6].Name", "OldName", "NewName"),
    new Distinction("Snapshot.Rules[3]", "Rule-3", "Rule-3 v2"),
    new Distinction("Snapshot.Metadata[isEnabled]", true, false),
    new Distinction("Snapshot.Metadata[range of values].Min", 10, 20),
    new Distinction(
        "Snapshot.Metadata[range of values].Bounds[1].Label", "Old bound", "New bound"),
    new Distinction("Snapshot.Portals[2]", null, 91, "Added"),
    new Distinction("Snapshot.Portals[3]", null, 101, "Added"),
    new Distinction("Snapshot.Portals[4]", 1000, null, "Removed"),
    new Distinction("Snapshot.Portals[0].Title", "Main Portal", "Main Portal v2"),
});

var json = DeepEqualsExtension.ToJson(distinctions);

/*
    {
      "Status": {
        "before": "Active",
        "after": "Deprecated",
        "details": "Different state"
      },
      "Rules": {
        "2": {
          "Expression": {
            "before": "Amount > 100",
            "after": "Amount > 200",
            "details": ""
          }
        },
        "6": {
          "Name": {
            "before": "OldName",
            "after": "NewName",
            "details": ""
          }
        },
        "3": {
          "before": "Rule-3",
          "after": "Rule-3 v2",
          "details": ""
        }
      },
      "Metadata": {
        "isEnabled": {
          "before": true,
          "after": false,
          "details": ""
        },
        "range of values": {
          "Min": {
            "before": 10,
            "after": 20,
            "details": ""
          },
          "Bounds": {
            "1": {
              "Label": {
                "before": "Old bound",
                "after": "New bound",
                "details": ""
              }
            }
          }
        }
      },
      "Portals": {
        "2": {
          "before": null,
          "after": 91,
          "details": "Added"
        },
        "3": {
          "before": null,
          "after": 101,
          "details": "Added"
        },
        "4": {
          "before": 1000,
          "after": null,
          "details": "Removed"
        },
        "0": {
          "Title": {
            "before": "Main Portal",
            "after": "Main Portal v2",
            "details": ""
          }
        }
      }
    }
*/
```

### Configuring the Comparison Pipeline

Prefer member-by-member comparison (property-level diffs) by skipping equality-based short-circuits. This is useful when types implement `==`, `Equals`, or `IComparable`, but you need detailed change tracking on each member.

```csharp
internal class CourseNew3
{
    public string Name { get; set; }
    public int Duration { get; set; }

    public static bool operator ==(CourseNew3 a, CourseNew3 b) => a?.Name == b?.Name;
    public static bool operator !=(CourseNew3 a, CourseNew3 b) => !(a == b);
    public override bool Equals(object? obj) => obj is CourseNew3 other && this == other;
    public override int GetHashCode() => Name?.GetHashCode() ?? 0;
}

var actual = new CourseNew3 { Name = "Math", Duration = 5 };
var expected = new CourseNew3 { Name = "Math", Duration = 4 };

var options = ComparatorOptions.SkipStrategies(
    StrategyType.Equality,
    StrategyType.OverridesEquals,
    StrategyType.CompareTo);

var diffs = expected.DeeplyEquals(actual, options);
// diffs[0].Path == "CourseNew3.Duration"
// diffs[0].ExpectedValue == 4
// diffs[0].ActualValue == 5
```

## Working with the Source

To build the solution locally:

```bash
dotnet restore
dotnet build
```

Run the included unit tests to verify changes:

```bash
dotnet test
```

The repository also contains performance benchmarks under `PerformanceTests` that can be executed to validate comparison throughput. Benchmarks typically take longer to run and may require release builds for accurate results.

## Contributing

Contributions are welcome! If you encounter an issue, have a question, or would like to suggest an improvement:

1. Search the existing issues to avoid duplicates.
2. Open a new issue with as much detail as possible (include sample objects, expected output, and actual output when relevant).
3. Fork the repository, create a feature branch, and submit a pull request with your changes.

Please ensure that new code is accompanied by tests and documentation updates where applicable.

## License

This project is licensed under the Apache-2.0 License. See the [LICENSE](LICENSE) file for more details.
