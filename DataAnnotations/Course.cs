using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAnnotations
{
    public class Course
    {
        public Course()
        {
            Tags = new HashSet<Tag>();
        }

        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; }

        public int Level { get; set; }

        public float FullPrice { get; set; }


        // On SQL, the Author property it's "Author_Id", if you want to get rid of the underline,
        // there are two steps need to follow: First you need to have a property that represents your
        // foreign key, and that property is going to have the desired name. And then we need to link that
        // property with the navigation property (the property on line 40 (Author));
        [ForeignKey("Author")] // the string is the name of the navigation property;
        public int AuthorId { get; set; }

        public virtual Author Author { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
    }
}