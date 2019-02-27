namespace SpaTemplate.Core.SharedKernel
{
	public interface ILinkDto
	{
		string Href { get; }
		string Rel { get; }
		string Method { get; }
	}
}