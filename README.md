# ObjectComparator

**This tool allows comparing objects furthermore provide distinctions. What is more, this tool can set compare rule for certain properties or fields and types.**

[![NuGet.org](https://img.shields.io/nuget/v/ObjectComparator.svg?style=flat-square&label=NuGet.org)](https://www.nuget.org/packages/ObjectComparator/)
[![Build Status](https://travis-ci.org/valeraf23/ObjectComparator.svg?branch=master)](https://travis-ci.org/valeraf23/ObjectComparator)
[![Build status](https://ci.appveyor.com/api/projects/status/1i6lq6mft1jy94vx/branch/master?svg=true)](https://ci.appveyor.com/project/valeraf23/objectcomparator/branch/master)
## Installation

#### Install with NuGet Package Manager Console
```
Install-Package ObjectComparator
```
#### Install with .NET CLI
```
dotnet add package ObjectComparator
```

## Example:

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
                
               var result = actual.GetDifferenceBetweenObjects(expected); 
	/*   
	    Property name "Name":
	    Expected Value :Alex
	    Actually Value :Bob
    
	    Property name "Vehicle.Model":
	    Expected Value :Audi
	    Actually Value :Opel
    
	    Property name "Courses[0].Duration":
	    Expected Value :04:00:00
	    Actually Value :03:00:00
    
	    Property name "Courses[1].Name":
	    Expected Value :Liter
	    Actually Value :Literature 
	*/
	    
```
   ## Set strategies for certain properties/fields
   
```csharp
         var result = actual.GetDifferenceBetweenObjects(expected,
                strategy => strategy
                    .Set(x => x.Vehicle.Model, (act, exp) => act.Length == exp.Length)
                    .Set(x => x.Courses[1].Name, (act, exp) => act.StartsWith('L') && exp.StartsWith('L')));           
        /* 
            Property name "Name":
            Expected Value :Alex
            Actually Value :Bob
            
            Property name "Courses[0].Duration":
            Expected Value :04:00:00
            Actually Value :03:00:00
        */
    
  ```

## Set Ignore list for properties/fields

```csharp

   var ignore = new[] {"Name", "Courses", "Vehicle" };
   var result = actual.GetDifferenceBetweenObjects(expected,ignore);
   
 /*
     There are no Distinction
 */
    
```

## Display distinctions for properties/fields which have the custom strategy

```csharp

     var result = actual.GetDifferenceBetweenObjects(expected,
                strategy => strategy
                    .Set(x => x.Vehicle.Model, (act, exp) => act.StartsWith('A') && exp.StartsWith('A')), "Name", "Courses");
    /*
 	 Name: Vehicle.Model
	 Expected Value :Audi
	 Actually Value :Opel
 	 LambdaExpression :(act, exp) => (act.StartsWith(A) AndAlso exp.StartsWith(A))
    */
    
    var skip = new[] {"Vehicle", "Name", "Courses[1].Name"};
            var result = expected.GetDifferenceBetweenObjects(actual,
                str => str.Set(x => x.Courses[0].Duration, (act, exp) => act > TimeSpan.FromHours(3),
                    new Display {Expected = "Expected that Duration should be more that 3 hours"}), skip);
    /*	    
	 Name: Courses[0].Duration
	 Expected Value :Expected that Duration should be more that 3 hours
	 Actually Value :04:00:00
	 LambdaExpression :(act, exp) => (act > 03:00:00)
   */
  
```

## Display distinctions for properties/fields which have a Dictionary type

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
                    ["Shantaram"] = new Book {Pages = 500, Text = "Shantaram Text"}
                }
            };

            var result = expected.GetDifferenceBetweenObjects(actual);
    /*
        Property name "Books[hobbit].Pages":
        Expected Value :1000
        Actually Value :1

        Property name "Books[murder in orient express].Text":
        Expected Value :murder in orient express Text
        Actually Value :murder in orient express Text1
   */
  
```
