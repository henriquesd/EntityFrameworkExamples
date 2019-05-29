using System.Data.Entity.ModelConfiguration;

namespace FluentAPI.EntityConfigurations
{
    public class CourseConfiguration : EntityTypeConfiguration<Course>
    {
        public CourseConfiguration()
        {
            // *the comments below are just sugestions;

            // change the name of the table;
            ToTable("tbl_Course");

            // override primary keys;
            HasKey(c => c.Id);

            // property configurations (sort then alphabetically);

            Property(c => c.Description)
                .IsRequired()
                .HasMaxLength(2000);

            Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(255);

            // relationships (sort then alphabetically);
            HasRequired(c => c.Author)
                .WithMany(a => a.Courses)
                .HasForeignKey(c => c.AuthorId)
                .WillCascadeOnDelete(false);

            HasRequired(c => c.Cover)
                .WithRequiredPrincipal(c => c.Course);

            HasMany(c => c.Tags)
            .WithMany(t => t.Courses)
            .Map(m =>
            {
                m.ToTable("CourseTags");
                m.MapLeftKey("CourseId");
                m.MapRightKey("TagId");
            });
        }
    }
}
