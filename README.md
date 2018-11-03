# ObjectsComparator

**This tool allows comparing objects furthermore provide distinctions. What is more, this tool can set compare rule for certain properties or fields and types.**

[![NuGet.org](https://img.shields.io/nuget/v/ObjectsComparator.svg?style=flat-square&label=NuGet.org)](https://www.nuget.org/packages/ObjectsComparator/)
[![Build Status](https://travis-ci.org/valeraf23/ObjectComparator.svg?branch=master)](https://travis-ci.org/valeraf23/ObjectComparator)

## Installation

#### Install with NuGet Package Manager Console
```
Install-Package ObjectsComparator
```
#### Install with .NET CLI
```
dotnet add package ObjectsComparator
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
