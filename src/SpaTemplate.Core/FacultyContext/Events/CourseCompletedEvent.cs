using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
    public class CourseCompletedEvent  : BaseDomainEvent
    {
        public CourseCompletedEvent(Course completedItem) => CompletedItem = completedItem;

        public Course CompletedItem { get; set; }
    }
}
