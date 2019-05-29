namespace FluentAPI
{
    public class Cover
    {
        // Each Entity needs to have a key or a primary key, otherwhise if you want to create
        // migration you're going to get an error.
        public int Id { get; set; }

        // Each cover should be associated with a course, so I had a navigation property here,
        // and need to create a relationship from Course to Cover (on Course class);
        public Course Course { get; set; }
    }
}
