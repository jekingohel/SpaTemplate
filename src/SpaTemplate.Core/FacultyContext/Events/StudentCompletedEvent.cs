using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
	public class StudentCompletedEvent : BaseDomainEvent
	{
		public StudentCompletedEvent(Student completedItem) => CompletedItem = completedItem;

		public Student CompletedItem { get; set; }
	}
}