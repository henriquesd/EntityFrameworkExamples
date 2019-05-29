using System.Data.Entity;

namespace FluentAPI
{
    public class PlutoContext : DbContext
    {
        public PlutoContext()
            : base("name=PlutoContext")
        {
        }

        public virtual DbSet<Author> Authors { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>()
            .Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(255);

            modelBuilder.Entity<Course>()
              .Property(c => c.Description)
              .IsRequired()
              .HasMaxLength(2000);

            // To configure the relationship between courses and authors, it doesn't matter which end
            // of the relationship you start;
            modelBuilder.Entity<Course>()
                .HasRequired(c => c.Author)
                .WithMany(a => a.Courses)
                .HasForeignKey(c => c.AuthorId)
                .WillCascadeOnDelete(false);

            // we can start from Course or from Tag, doesn't matter;
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Tags)
                .WithMany(t => t.Courses)
                .Map(m =>
                {
                    m.ToTable("CourseTags");
                    m.MapLeftKey("CourseId");
                    m.MapRightKey("TagId");
                 });

            modelBuilder.Entity<Course>()
                .HasRequired(c => c.Cover)
                .WithRequiredPrincipal(c => c.Course);

            base.OnModelCreating(modelBuilder);
        }
    }
}