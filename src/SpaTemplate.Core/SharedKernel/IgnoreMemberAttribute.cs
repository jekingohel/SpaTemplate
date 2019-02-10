using System;

namespace SpaTemplate.Core
{
	// source: https://github.com/jhewlett/ValueObject
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class IgnoreMemberAttribute : Attribute
	{
	}
}