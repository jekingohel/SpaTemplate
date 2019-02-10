namespace SpaTemplate.Core
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
		public const string PeopleApi = RootApi + "/people";
		public const string CoursesApi = PeopleApi + "/{personId}/courses";
		public const string PersonCollectionsApi = RootApi + "/personcollections";
	}

	public static class RouteName
	{
		public const string GetRoot = "GetRoot";

		public const string GetPeople = "GetPeople";
		public const string GetPerson = "GetPerson";
		public const string DeletePerson = "DeletePerson";
		public const string CreatePerson = "CreatePerson";
		public const string PartiallyUpdatePerson = "PartiallyUpdatePerson";

		public const string GetPersonCollection = "GetPersonCollection";

		public const string GetCoursesForPerson = "GetCoursesForPerson";
		public const string DeleteCourseForPerson = "DeleteCourseForPerson";
		public const string UpdateCourseForPerson = "UpdateCourseForPerson";
		public const string PartiallyUpdateCourseForPerson = "PartiallyUpdateCourseForPerson";
		public const string CreateCourseForPerson = "CreateCourseForPerson";
		public const string GetCourseForPerson = "GetCourseForPerson";
	}

	public static class Rel
	{
		public const string Self = "self";

		public const string People = "people";
		public const string CreatePerson = "create_person";
		public const string PatchPerson = "patch_person";
		public const string DeletePerson = "delete_person";

		public const string NextPage = "nextPage";
		public const string PreviousPage = "previousPage";

		public const string PartiallyUpdateCourse = "partiallyUpdate_course";
		public const string UpdateCourse = "update_course";
		public const string DeleteCourse = "delete_course";
		public const string CreateCourseForPerson = "create_course";
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
		public const string InputFormatterJson = "application/vnd.xeinaemm.person.full+json";
		public const string OutputFormatterJson = "application/vnd.xeinaemm.hateoas+json";
		public const string PatchFormatterJson = "application/json-patch+json";
	}
}