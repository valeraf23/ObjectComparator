# ObjectComparator

**This tool allows comparing objects furthermore provide distinctions. What is more, this tool can set compare strategy for certain properties or fields.**

## Compare objects

 

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
                        new Cours
                        {
                            Name = "Math",
                            Duration = TimeSpan.FromHours(4)
                        },
                        new Cours
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
                    Courses = new []
                    {
                        new Cours
                        {
                            Name = "Math",
                            Duration = TimeSpan.FromHours(3)
                        },
                        new Cours
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
   ## Define strategies 
   

        var result = actual.GetDifferenceBetweenObjects(expected,
        strategy => strategy
       .Set(x => x.Vehicle.Model,(act, exp) => act.Length == exp.Length)
       .Set(x => x.Courses[1].Name, (act, exp) => act.StartsWith('L') && exp.StartsWith('L')));
           
           /* 
            Property name "Name":
            Expected Value :Alex
            Actually Value :Bob
            
            Property name "Courses[0].Duration":
            Expected Value :04:00:00
            Actually Value :03:00:00
            */
    
    

## Ignore 

     var ignore = new[] {"Name", "Courses", "Vehicle" };
     var result = actual.GetDifferenceBetweenObjects(expected,ignore);
    /*
    There are no Distinction
    */
