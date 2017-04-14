# MonGen

Monadic data generatorcs in C#


Example:

```CSharp
   public class Person
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public DateTime Birthday { get; set; }
   }

   public static IGenerator<Person> Gen =
       from i in Generators.Range(0, 100)
       from n in Generators.Regex("[A-Z][a-z]{2,16}")
       from d in Generators.Range(DateTime.Parse("1930-01-01T00:00:00Z"), DateTime.Parse("2017-01-01T00:00:00Z"))
       select new Person { Id = i, Name = n, Birthday = d };
```
