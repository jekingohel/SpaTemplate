namespace SpaTemplate.Core.SharedKernel
{
	public static class Constants
	{
		public const string KeyLink = "links";
		public const string LocalhostIp = "127.0.0.1";
	}

	public static class Method
	{
		public const string Get = "GET";
		public const string Post = "POST";
		public const string Patch = "PATCH";
		public const string Put = "PUT";
		public const string Delete = "DELETE";
	}

	public static class Route
	{
        public const string RootApi = "api";
        public const string PeopleApi = "api/people";
        public const string CoursesApi = "api/people/{studentId}/courses";
        public const string StudentCollectionsApi = "api/studentcollections";
    }

    public static class Api
    {
        public static string People =  "api/people";
        public static string Student = "api/people/{0}";
        public static string Courses = "api/people/{0}/courses";
        public static string Course =  "api/people/{0}/courses/{1}";
        public static string StudentCollections = "api/studentcollections";
        public static string StudentCollectionsIds = "api/studentcollections/({0})";
    }

	public static class RouteName
	{
		public const string GetRoot = "GetRoot";

		public const string GetPeople = "GetPeople";
		public const string GetStudent = "GetStudent";
		public const string DeleteStudent = "DeleteStudent";
		public const string CreateStudent = "CreateStudent";
		public const string PartiallyUpdateStudent = "PartiallyUpdateStudent";

		public const string GetStudentCollection = "GetStudentCollection";

		public const string GetCoursesForStudent = "GetCoursesForStudent";
		public const string DeleteCourseForStudent = "DeleteCourseForStudent";
		public const string UpdateCourseForStudent = "UpdateCourseForStudent";
		public const string PartiallyUpdateCourseForStudent = "PartiallyUpdateCourseForStudent";
		public const string CreateCourseForStudent = "CreateCourseForStudent";
		public const string GetCourseForStudent = "GetCourseForStudent";
	}

	public static class Rel
	{
		public const string Self = "self";

		public const string People = "people";
		public const string CreateStudent = "create_student";
		public const string PatchStudent = "patch_student";
		public const string DeleteStudent = "delete_student";

		public const string NextPage = "nextPage";
		public const string PreviousPage = "previousPage";

		public const string PartiallyUpdateCourse = "partiallyUpdate_course";
		public const string UpdateCourse = "update_course";
		public const string DeleteCourse = "delete_course";
		public const string CreateCourseForStudent = "create_course";
	}

	public static class Header
	{
		public const string Accept = "Accept";
		public const string XPagination = "X-Pagination";
		public const string ContentType = "Content-Type";
		public const string XRealIp = "X-Real-IP";
	}

	public static class MediaType
	{
		public const string InputFormatterJson = "application/vnd.xeinaemm.student.full+json";
		public const string OutputFormatterJson = "application/vnd.xeinaemm.hateoas+json";
		public const string PatchFormatterJson = "application/json-patch+json";
	}
}