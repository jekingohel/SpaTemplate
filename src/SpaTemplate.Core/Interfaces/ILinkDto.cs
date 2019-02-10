namespace SpaTemplate.Core
{
	public interface ILinkDto
	{
		string Href { get; }
		string Rel { get; }
		string Method { get; }
	}
}