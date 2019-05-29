namespace CodeFirstExistingDataBase
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Courses")]
    public partial class Course
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course()
        {
            Tags = new HashSet<Tag>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        // Can use data annotation \/, or Fluent API on PlutoContext;
        // [Required]
        public string Description { get; set; }
        // public Category Category { get; set; }

        // The property below was created and deleted;
        // public DateTime? DatePublished { get; set; }

        public int Level { get; set; }

        public float FullPrice { get; set; }

        public int? Author_Id { get; set; }

        public virtual Author Author { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tag> Tags { get; set; }
    }
}
