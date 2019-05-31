using System;
using System.Linq;

namespace Queries
{
    class Program
    {
        static void Main(string[] args)
        {
            linqExampleOne();
            Console.WriteLine("-------------");

            linqExampleTwo();
            Console.WriteLine("-------------");

            linqExampleJoins();
            Console.WriteLine("-------------");

            linqExampleExtensionMethods();
            Console.WriteLine("-------------");

            ExampleOtherMethodsNotSupportedByLINQSyntax();

            ExampleDeferredExecution();
            Console.WriteLine("-------------");

            ExampleIQueryable();
            Console.WriteLine("-------------");


            Console.ReadLine();
        }

        private static void ExampleIQueryable()
        {
            // IQueryable it's an interface that derives from IEnumerable;

            // IQueryable allows queries to be extended without being immediately executed;

            var context = new PlutoContext();

            IQueryable<Course> courses = context.Courses;
            // IEnumerable<Course> courses = context.Courses;
            var filtered = courses.Where(c => c.Level == 1);

            foreach (var course in filtered)
                Console.WriteLine(course.Name); // can see in SQL Profiler, and note that the filter is part ot the query, and that's
                                                // because here (above) we are using IQueryable;
                                                // If we use IEnumerable insted of IQueryable, the result will be the same, but in the query we will not
                                                // have the where clause, it will be only selected from courses. This means that you're going to get
                                                // all courses in the database even if there are a billion of those courses, load them into memory
                                                // and then apply the filter;
                                                // When you use IEnumerable, you cannot extend a query;
        }

        private static void ExampleDeferredExecution()
        {
            // Queries are not executed at the time you create them;

            // Query executed when:
            // - Iterating over query variable;
            // - Calling ToList, ToArray, ToDictionary
            // - Calling First, Last, Single, Count, Max, Min, Average

            var context = new PlutoContext();

            var courses = context.Courses;
            var filtered = courses.Where(c => c.Level == 1);
            var sorted = filtered.OrderBy(c => c.Name);

            var courses2 = context.Courses.Where(c => c.Level == 1).OrderBy(c => c.Name);

            foreach (var c in courses)
                Console.WriteLine(c.Name); // the query will be executed here; you can use SQL Profiler to see it;

            Console.WriteLine("-------------");

            // IsBeginnerCourse is a custom property;
            // immediate execution;
            // Without "ToList()" will thrown an error;
            var courses3 = context.Courses.ToList().Where(c => c.IsBeginnerCourse == true);

            foreach (var c in courses3)
                Console.WriteLine(c.Name);

        }

        private static void ExampleOtherMethodsNotSupportedByLINQSyntax()
        {
            var context = new PlutoContext();

            // Partitioning; - This is pretty useful when you want to return a page of records;
            var courses = context.Courses.Skip(10).Take(10);

            // Element Operators;
            //context.Courses.First...
            context.Courses.OrderBy(c => c.Level).FirstOrDefault(c => c.FullPrice > 100);
            // context.Courses.Last   - The last method cannot be applied when you are working with a database like SQL server;
            // in SQL we don't have an operator to get the last record in a table, so if you want the last record you need to 
            // sort them in a descending way and then select the first record;
            context.Courses.SingleOrDefault(c => c.Id == 1); // this is used to return only a single object;

            // Quantifying;
            var allAbove10Dollars = context.Courses.All(c => c.FullPrice > 10);
            context.Courses.Any(c => c.Level == 1);

            // Aggregating;
            var count = context.Courses.Count();
            context.Courses.Max(c => c.FullPrice);
            context.Courses.Min(c => c.FullPrice);
            context.Courses.Average(c => c.FullPrice);

            var count2 = context.Courses.Where(c => c.Level == 1).Count();
        }

        private static void linqExampleExtensionMethods()
        {
            var context = new PlutoContext();

            // Restriction;
            var courses = context.Courses.Where(c => c.Level == 1);

            // Ordering;
            // If you want to order with multiple properties use ThenBy..
            var courses2 = context.Courses
                .Where(c => c.Level == 1)
                .OrderByDescending(c => c.Name)
                .ThenByDescending(c => c.Level);

            // Projection;
            var courses3 = context.Courses
               .Where(c => c.Level == 1)
               .OrderByDescending(c => c.Name)
               .ThenByDescending(c => c.Level)
               .Select(c => new { CourseName = c.Name, AuthorName = c.Author.Name });

            var tags = context.Courses
               .Where(c => c.Level == 1)
               .OrderByDescending(c => c.Name)
               .ThenByDescending(c => c.Level)
               .Select(c => c.Tags);

            foreach (var t in tags)
            {
                foreach (var tag in t)
                    Console.WriteLine(tag.Name);
            }
            Console.WriteLine("-------------");

            var tags2 = context.Courses
              .Where(c => c.Level == 1)
              .OrderByDescending(c => c.Name)
              .ThenByDescending(c => c.Level)
              .SelectMany(c => c.Tags);

            foreach (var t in tags2)
                Console.WriteLine(t.Name);

            Console.WriteLine("-------------");


            // Set Operators;
            var tags3 = context.Courses
            .Where(c => c.Level == 1)
            .OrderByDescending(c => c.Name)
            .ThenByDescending(c => c.Level)
            .SelectMany(c => c.Tags)
            .Distinct();

            foreach (var t in tags3)
                Console.WriteLine(t.Name);

            Console.WriteLine("-------------");


            // Grouping;
            var groups = context.Courses.GroupBy(c => c.Level);

            foreach (var group in groups)
            {
                Console.WriteLine("Key: " + group.Key);

                foreach (var course in group)
                    Console.WriteLine("\t" + course.Name);
            }

            // Joining;
            context.Courses.Join(context.Authors,
                c => c.AuthorId,
                a => a.Id,
                (course, author) => new
                    {
                        CourseName = course.Name,
                        AuthorName = author.Name
                    });

            // Group Join;
            context.Authors.GroupJoin(context.Courses, a => a.Id, c => c.AuthorId, (author, course) => new
            {
                //AuthorName = author.Name,
                //Courses = courses
                Author = author,
                Courses = courses.Count()
            });

            // Cross Join
            context.Authors.SelectMany(a => context.Courses, (author, course) => new
            {
                AuthorName = author.Name,
                CourseName = course.Name
            });
        }

        private static void linqExampleJoins()
        {
            var context = new PlutoContext();

            // Joins;
            // We have three types of joins in link: Inner Join, Group Join and Cross Join;

            var query =
                from c in context.Courses
                select new { CourseName = c.Name, AuthorName = c.Author.Name }; // using the navigation property "Author";

            // Inner Join - Use when there is no relationship between your entities and you need to link them based on a key.
            var query2 =
                 from c in context.Courses
                 join a in context.Authors on c.AuthorId equals a.Id
                 select new { CourseName = c.Name, AuthorName = a.Name }; // without using the navigation property "Author";

            // Group Join - Useful when you need to group objects by a property and count the number of objects in each group.
            // In SQL we do this with LEFT JOIN, COUNT(*) and GROUP BY.In LINQ, we use group join
            var query3 =
                from a in context.Authors
                join c in context.Courses on a.Id equals c.AuthorId into g // when you use into g, the result will be a group join;
                select new { AuthorName = a.Name, Courses = g.Count() };

            foreach (var x in query3)
                Console.WriteLine("{0} ({1})", x.AuthorName, x.Courses);

            Console.WriteLine("-------------");

            // Cross Join - To get full combinations of all objects on the left and the ones on the right. 
            var query4 =
                from a in context.Authors
                from c in context.Courses
                select new { AuthorName = a.Name, CourseName = c.Name };

            foreach (var x in query4)
                Console.WriteLine("{0} - {1}", x.AuthorName, x.CourseName);
        }

        private static void linqExampleTwo()
        {
            var context = new PlutoContext();

            // Restriction;
            var queryRestriction =
                from c in context.Courses
                where c.Level == 1 && c.Author.Id == 1
                select c;

            // Ordering;
            var query2 =
              from c in context.Courses
              where c.Author.Id == 1
              orderby c.Level descending, c.Name
              select c;

            // Projection;
            var query3 =
             from c in context.Courses
             where c.Author.Id == 1
             orderby c.Level descending, c.Name
             select new { Name = c.Name, Author = c.Author.Name };

            // Grouping;
            var queryGroupByLevel =
                from c in context.Courses
                group c by c.Level
                into g
                select g;

            foreach (var group in queryGroupByLevel)
            {
                Console.WriteLine(group.Key);

                foreach (var course in group)
                    Console.WriteLine("\t{0}", course.Name);
            }
            Console.WriteLine("-------------");

            foreach (var group in queryGroupByLevel)
            {
                Console.WriteLine("{0} ({1})", group.Key, group.Count());
            }
        }

        private static void linqExampleOne()
        {
            var context = new PlutoContext();

            // LINQ syntax;
            var query =
                from c in context.Courses
                where c.Name.Contains("c#")
                orderby c.Name
                select c;

            foreach (var course in query)
                Console.WriteLine(course.Name);


            Console.WriteLine("-------------");

            // Extension methods;
            var courses = context.Courses
                .Where(c => c.Name.Contains("c#"))
                .OrderBy(c => c.Name);

            foreach (var course in courses)
                Console.WriteLine(course.Name);
        }
    }
}
