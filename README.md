# ObjectComparator

**This tool allows comparing objects furthermore provide distinctions. What is more, this tool can set compare strategy for certain properties or fields.**


## Comparing objects

     var actual= new Student
                {
                    Name = "Alex",
                    Age = 20
                };
    
                var exp = new Student
                {
                    Name = "Bob",
                    Age = 21             
                };
        var result = actual.GetDifferenceBetweenObjects(expected);
        result.ToString();
     /*  
    Property name "Name":
    Expected Value :Alex
    Actually Value :Bob
    
    Property name "Age":
    Expected Value :20
    Actually Value :21
    */
