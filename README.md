# ObjectComparator

## Overview

ObjectComparator is a high-performance .NET library crafted meticulously for deep comparisons between objects. Beyond just pointing out the disparities, it delves into the depths of objects, reflecting even the minutest differences. Furthermore, it arms developers with the capability to prescribe custom comparison rules tailored for specific properties or fields.


## Key Features
- **Deep Comparisons:** Unravel every detail and identify even the slightest differences between complex objects.
- **Custom Rules:** Define bespoke comparison criteria for distinct properties or fields, ensuring you have full control over the comparison process.
- **High Performance:** Despite the comprehensive nature of its comparisons, ObjectComparator stands out due to its blazing speed.


[![NuGet.org](https://img.shields.io/nuget/v/ObjectComparator.svg?style=flat-square&label=NuGet.org)](https://www.nuget.org/packages/ObjectComparator/)  
![Nuget](https://img.shields.io/nuget/dt/ObjectComparator)  
[![Build status](https://ci.appveyor.com/api/projects/status/1i6lq6mft1jy94vx/branch/master?svg=true)](https://ci.appveyor.com/project/valeraf23/objectcomparator/branch/master)  
[![.NET Actions Status](https://github.com/valeraf23/ObjectComparator/workflows/.NET/badge.svg)](https://github.com/valeraf23/ObjectComparator/actions)

## Table of Contents
- [Overview](#overview)
- [Key Features](#key-features)
- [Installation](#installation)
- [Usage](#usage)
- [Display Distinctions with Custom Strategy](#display-distinctions-with-custom-strategy)
  - [Comparison for Collection Types](#comparison-for-collection-types)   
  - [Comparison for Dictionary Types](#comparison-for-dictionary-types)
  - [Ignore Strategy](#ignore-strategy)
  - [DeeplyEquals if type (not primitives and not Anonymous Type) has Overridden Equals method](#deeplyequals-if-type-not-primitives-and-not-anonymous-type-has-overridden-equals-method)
  - [DeeplyEquals if type has Overridden Equality method](#deeplyequals-if-type-has-overridden-equality-method)
  - [Display distinctions for Dictionary type](#display-distinctions-for-dictionary-type)
  - [Comparison for Anonymous Types](#comparison-for-anonymous-types)
  - [Convert Comparison Result to JSON](#convert-comparison-result-to-JSON)
  - [Configuring comparison pipeline (ComparatorOptions)](#configuring-comparison-pipeline-(ComparatorOptions))
- [Contributing](#contributing)
- [License](#license)

## Installation

### NuGet Package Manager Console
```bash
Install-Package ObjectComparator
```

#### Install with .NET CLI
```
dotnet add package ObjectComparator
```

## Usage

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

  ### Custom Strategies for Comparison
  
  Define specific strategies for comparing properties.
   
```csharp
         var result = actual.DeeplyEquals(expected,
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

### Ignoring Specific Properties or Fields

   Omit certain properties or fields from the comparison.

```csharp

    var ignore = new[] {"Name", "Courses", "Vehicle" };
    var result = actual.DeeplyEquals(expected,ignore);
   
     /*
     	Objects are deeply equal
    */
    
```

### Display Distinctions with Custom Strategy

   Provide specific strategies and display the differences.
   
```csharp

     var result = actual.DeeplyEquals(expected,
                strategy => strategy
                    .Set(x => x.Vehicle.Model, (act, exp) => act.StartsWith('A') && exp.StartsWith('A')), "Name", "Courses");
		    
    /*
		Path: "Student.Vehicle.Model":
		Expected Value :Audi
		Actually Value :Opel
		Details : (act:(Audi), exp:(Opel)) => (act:(Audi).StartsWith(A) AndAlso exp:(Opel).StartsWith(A))
    */
    
    var skip = new[] {"Vehicle", "Name", "Courses[1].Name"};
            var result = expected.DeeplyEquals(actual,
                str => str.Set(x => x.Courses[0].Duration, (act, exp) => act > TimeSpan.FromHours(3),
                    new Display {Expected = "Expected that Duration should be more that 3 hours"}), skip);
		    
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
                    ["hobbit"] = new Book {Pages = 1000, Text = "hobbit Text"},
                    ["murder in orient express"] = new Book {Pages = 500, Text = "murder in orient express Text"},
                    ["Shantaram"] = new Book {Pages = 500, Text = "Shantaram Text"}
                }
            };

            var actual = new Library
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new Book {Pages = 1, Text = "hobbit Text"},
                    ["murder in orient express"] = new Book {Pages = 500, Text = "murder in orient express Text1"},
                    ["Shantaram"] = new Book {Pages = 500, Text = "Shantaram Text"},
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

### DeeplyEquals if type(not primities and not Anonymous Type) has Overridden Equals method

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

### DeeplyEquals if type has Overridden Equality  method

```csharp		
	/*	
		Path: "SomeTest":
		Expected Value :ObjectsComparator.Tests.SomeTest
		Actually Value :ObjectsComparator.Tests.SomeTest
		Details : == (Equality Operator)
	*/
```	

### Display distinctions for Dictionary type
	
```csharp
	var firstDictionary = new Dictionary<string, string>
            {
                {"Key", "Value"},
                {"AnotherKey", "Value"},
            };

        var secondDictionary = new Dictionary<string, string>
            {
                {"Key", "Value"},
                {"AnotherKey", "AnotherValue"},
            };
			
	var result = firstDictionary.DeeplyEquals(secondDictionary)
			 
			 
	/*	
		Path: "Dictionary<String, String>[AnotherKey]":
		Expected Value :Value
		Actually Value :AnotherValue
	*/
	
```				

### Comparison for Anonymous Types

Detect differences when dealing with anonymous types.
		
```csharp
            var actual = new {Integer = 1, String = "Test", Nested = new byte[] {1, 2, 3}};
	    var expected = new {Integer = 1, String = "Test", Nested = new byte[] {1, 2, 4}};
			
	    var result = exp.DeeplyEquals(act);
			
	/*
		Path: "AnonymousType<Int32, String, Byte[]>.Nested[2]":
		Expected Value :3
		Actually Value :4
	*/
                
```

### Convert Comparison Result to JSON

You can serialize the result of object comparison (DeepEqualityResult) into a structured JSON format, suitable for logging, UI display, or audits.
		
```csharp
	var distinctions = DeepEqualityResult.Create(new[]
	{
	    new Distinction("Snapshot.Rules[2].Expression", "Amount > 100", "Amount > 200"),
	    new Distinction("Snapshot.Rules[6].Name", "OldName", "NewName"),
	    new Distinction("Snapshot.Portals", null, 91, "Added"),
	    new Distinction("Snapshot.Portals", null, 101, "Added"),
	    new Distinction("Snapshot.Portals", 1000, null, "Removed"),
	    new Distinction("Snapshot.Portals[0].Title", "Main Portal", "Main Portal v2"),
	});
	
	var json = DeepEqualsExtension.ToJson(distinctions);
			
	/*
		{
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
		    }
		  },
		  "Portals": {
		    "Added": {
		      "before": null,
		      "after": 101,
		      "details": "Added"
		    },
		    "Removed": {
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

## Configuring comparison pipeline (ComparatorOptions)

Prefer member-by-member comparison (property-level diffs) by skipping equality-based short-circuits. This is useful when types implement `==`, `Equals`, or `IComparable`, but you need detailed change tracking on each member.

```
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
	
	var options = ComparatorOptions.Create(StrategyType.Equality, StrategyType.OverridesEquals, StrategyType.CompareTo);

	var diffs = expected.DeeplyEquals(actual, options);
	// diffs[0].Path == "CourseNew3.Duration"
	// diffs[0].ExpectedValue == 4
	// diffs[0].ActualValue == 5

```

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request on GitHub.

## License

This project is licensed under the Apache-2.0 License. See the [LICENSE](LICENSE) file for more details.
