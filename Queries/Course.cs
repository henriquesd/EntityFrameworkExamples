using System.Collections.Generic;

namespace Queries
{
    public class Course
    {
        public Course()
        {
            Tags = new HashSet<Tag>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Level { get; set; }

        public float FullPrice { get; set; }

        public virtual Author Author { get; set; }

        public int AuthorId { get; set; }

        // virtual it's the key to enable lazy loading on this property;
        // you can disabled Lazy Loading, by not declaring your properties as virtual,
        // or using a configuration on context (see PlutoContext);
        public virtual ICollection<Tag> Tags { get; set; }

        public Cover Cover { get; set; }

        public bool IsBeginnerCourse
        {
            get { return Level == 1; }
        }
    }
}
