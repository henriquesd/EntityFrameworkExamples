using System;
using System.Data.Entity;
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

            // In web applications stay away from lazy loading because in most cases you need to know
            // ahead of time what data exactly you need to return to the client;
            // So in web application it's better to use Either Loading or Explicit Loading;
            ExampleLazyLoading();
            Console.WriteLine("-------------");

            // To execute this method, comment the Configuration.LazyLoadingEnabled line on PlutoContext.cs;
            //ExampleNPlus1Problem();
            //Console.WriteLine("-------------");

            ExampleEagerLoading();
            Console.WriteLine("-------------");

            // There is also a third way to load related objects, which is called Explicit Loading,
            // which can be useful in situations where you still need to load a lot of objects,
            // but your queries are getting too complex;
            ExampleExplicitLoading();
            Console.WriteLine("-------------");

            ExampleAddingObjects();
            Console.WriteLine("-------------");

            ExampleUpdatingObjects();
            Console.WriteLine("-------------");

            //ExampleRemovingObjects();
            //Console.WriteLine("-------------");

            //ExampleWorkingWithChangeTracker();
            //Console.WriteLine("-------------");

            // LINQPad - https://www.linqpad.net/
            // With LINQPad you can see the exact query that is going to be executed by Entity Framework, similar to SQL Profiler
            // SQL Profiler is useful if you're navigating your application and you want to see what queries are being executed on SQL Server;
            // But if you're not navigating your application, you can copy and paste your queries from your source code into LINQ Pad, and run them;
            // LINQPad is not limited to link and Entity Framework, we can write any C# code and get immediate feedback.

            Console.ReadLine();
        }

        private static void ExampleWorkingWithChangeTracker()
        {
            var context = new PlutoContext();

            // Add an object
            context.Authors.Add(new Author { Name = "New Author 1" });

            // Update an object
            var author = context.Authors.Find(6);
            author.Name = "Updated";

            // Remove an object
            var another = context.Authors.Find(6);
            context.Authors.Remove(another);

            var entries = context.ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                // Reload this can sometimes be useful if you have edited an entry, but you change your mind and you want
                // to reload it from the database;
                // entry.Reload();

                // can debug to edit the values if you want and see the properties;
                Console.WriteLine(entry.State);
            }
        }

        private static void ExampleRemovingObjects()
        {
            var context = new PlutoContext();

            // With cascade delete;
            var course = context.Courses.Find(10);
            context.Courses.Remove(course);

            context.SaveChanges();
            // can see in SQL Profiler;

            // ------------------------------------

            // Without cascade delete;

            // This will throw an exception (from SQL);
            //var author = context.Authors.Find(6);
            //context.Authors.Remove(author);

            // So in this case we need to explicitly delete these courses first, and then delete the author;
            var author = context.Authors.Include(a => a.Courses).Single(a => a.Id == 7);

            // Use RemoveRang if you want to remove a list of objects - it accepts an IEnumerable object,
            // so this way you don't have to use a foreach block and call the remove method on each object;
            context.Courses.RemoveRange(author.Courses);
            context.Authors.Remove(author);

            context.SaveChanges();

            // can se in SQL Profiler;

            // -----------------------

            // Best Praticles
            // Prefer local deletes to physical deletes
            // author.IsDeleted = true;
        }

        private static void ExampleUpdatingObjects()
        {
            var context = new PlutoContext();

            // The Find method we use it to look up objects by their primary key,
            // it's a short way of writing this: Single(c => c.Id == 4);
            // and if your records have composite primary keys, you can pass multiple values like this:
            //var course = context.Courses.Find(4, 1, 2);
            var course = context.Courses.Find(4);
            course.Name = "Javascript: Understanding the Weird Parts 2";
            course.AuthorId = 2;

            context.SaveChanges();
        }

        private static void ExampleAddingObjects()
        {
            var context = new PlutoContext();

            #region initial code
            var course = new Course
            {
                Name = "New Course",
                Description = "New Description",
                FullPrice = 19.95f,
                Level = 1,
                Author = new Author { Id = 1, Name = "Mosh Hamedani" } // here we're instantiating a new author, because of that it will created
                                                                       // a new author on database; the change tracker sees this as a new object;
                                                                       // it has no knowledge that we have an existing author in the database with ID 1 and name "Mosh Hamedani";
                                                                       // there is two ways to resolve this, see these ways on the code below;
            };

            //context.Courses.Add(course);

            //context.SaveChanges();
            #endregion
           

            // The first approach - WPF applications;
            var authorsFirstApproach = context.Authors.ToList();
            var author = context.Authors.Single(a => a.Id == 1);

            var courseFirstApproach = new Course
            {
                Name = "New Course 2",
                Description = "New Description",
                FullPrice = 19.95f,
                Level = 1,
                Author = author
            };

            //context.Courses.Add(courseFirstApproach);
            //context.SaveChanges();

            // The second approach - this fits approach better in web applications; (can use in WPF too);
            var courseSecondApproach = new Course
            {
                Name = "New Course 3",
                Description = "New Description",
                FullPrice = 19.95f,
                Level = 1,
                AuthorId = 1
            };

            context.Courses.Add(courseSecondApproach);
            context.SaveChanges();

            // There's also a third way to solve this problem, which is not very common:
            // for example, if we have an object that is not in our context;
            // *not recommended;
            //var author2 = new Author() { Id = 1, Name = "Mosh Hamedani" };

            //context.Authors.Attach(author2);

            //var courseThirdApproach = new Course
            //{
            //    Name = "New Course 4",
            //    Description = "New Description",
            //    FullPrice = 19.95f,
            //    Level = 1,
            //    Author = author2
            //};

            //context.Courses.Add(courseThirdApproach);
            //context.SaveChanges();

            // -----------------------

            // Associating Objects

            // Using an existing object in context;
            //course.Author = context.Authors.Single(a => a.Id == 1);

            // Using foreign key properties;
            //course.AuthorId = 1;
        }

        private static void ExampleExplicitLoading()
        {
            // Here we get the code of Eager Loading, and modified the code to use Explicit Loading;

            // You can run the application and see the queries in SQL Profiler;

            var context = new PlutoContext();

            // The Include was removed, and this is going to simplify my query, because EF is not
            // going to join the Authors table with the Courses table;
            // And then we need to explicity load the courses for this author; and this is what we call Explicit Loading;
            var author = context.Authors.Single(a => a.Id == 1);

            // There are two ways to do Explicit Loading:

            // MSDN way (this only works for single entries - for example, here he have only one author object):
            context.Entry(author).Collection(a => a.Courses).Load();

            // Other way (Mosh way):
            context.Courses.Where(c => c.AuthorId == author.Id).Load();

            foreach (var course in author.Courses)
            {
                Console.WriteLine("{0}", course.Name);
            }

            // With the Explicit Loading we can simplify the complex query that is result of too many
            // includes and too much Eager Loading; Of course, we're going to end up with multiple round
            // trips to the database, but sometimes this can be more efficient than running a huge query
            // on the database;

            // But Explicit Loading also has another benefit: we can apply filters to the related objects:

            // MSDN Way:
            context.Entry(author).Collection(a => a.Courses).Query().Where(c => c.FullPrice == 0).Load();

            // Other way (Mosh way):
            context.Courses.Where(c => c.AuthorId == author.Id && c.FullPrice == 0).Load();

            // ------------------------------------------------------------------------------

            var authors = context.Authors.ToList();

            // Prefer to use the second approach - the Mosh way - because it's simpler and it's more flexible;
            // for example, to get the free courses for these authors, we cannot use the MSCN approach, because
            // if I call context.Entry, I cannot pass the authors object, because the Entry method is used to
            // reference only a single entry, a single object;
            //context.Entry()...

            // with the other approach, see:
            var authorIds = authors.Select(a => a.Id);

            context.Courses.Where(c => authorIds.Contains(c.AuthorId) && c.FullPrice == 0).Load();
        }

        private static void ExampleEagerLoading()
        {
            // Eager Loading is the opposite of Lazy Loading;

            var context = new PlutoContext();

            // You can run the application and see the query in SQL Profiler;

            // using the example of ExampleNPlus1Problem method, we can solve this N + 1 issue by
            // eager loading all the courses and their authors;
            var courses = context.Courses.Include("Author").ToList(); //* The Course class has a property called Author;
            // note that ".Include("Author") was added; With this, the query that Entity Framework generates
            // will join the Courses table with the Authors table to eager load all the courses and their authors;
            
            // *... .Include("Author") it's not a good praticle;
            // The problem with this "magic string" ("Author"), is if tomorrow I renamed this Author property
            // to something like "Teacher" or "Instructor", then this field is gonna break;
            foreach (var course in courses)
            {
                Console.WriteLine("{0} by {1}", course.Name, course.Author.Name);
            }
            Console.WriteLine("-------------");

            // * A better way to do that, it's using with lambda expression:
            // With this lambda expression, if I rename this Author property to something else like
            // instructor, because this lambda expression is going to be compiled, we're going to get
            // a compile time error;
            var courses2 = context.Courses.Include(c => c.Author).ToList();

            foreach (var course in courses2)
            {
                Console.WriteLine("{0} by {1}", course.Name, course.Author.Name);
            }


            //** How to use Eager Loading with Multiple Levels:

            // For single properties:
            //context.Courses.Include(c => c.Author.Address);

            // For collection properties:
            //context.Courses.Include(a => a.Tags.Select(t => t.Moderator));
        }

        private static void ExampleNPlus1Problem()
        {
            // To get N entities and their related entities, we'll end up with N + 1 queries;
            var context = new PlutoContext();

            // In terms of execution, when we run this line here, and the framework is going to
            // send a query to the database to get all courses; So this is going to be one query;
            var courses = context.Courses.ToList();

            foreach (var course in courses)
            {
                // For each course, because the Author property is not initialy loaded, here lazy load
                // kicks in; and for each course Entity Framework is going to run a separate query to get
                // the author for that course;
                // So assuming we have N courses we'll end up with N extra queries in this for loop, so N + 1;
                // You can execute the project, and debug, and see in the SQL Profiler;
                Console.WriteLine("{0} by {1}", course.Name, course.Author.Name);
            }
        }

        private static void ExampleLazyLoading()
        {
            var context = new PlutoContext();

            var course = context.Courses.Single(c => c.Id == 2);
            // se Tags property on Course class;

            // debug and see on SQL Profiler;
            // debugging, here, see the watch window, it gives us the illusion that the tags are loaded
            // immediately ahead of time, not inside the foreach block, because currently we are not
            // inside the fourth block yet; so de watch window in the debugger can be misleading;
            foreach (var tag in course.Tags)
                Console.WriteLine(tag.Name);
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
