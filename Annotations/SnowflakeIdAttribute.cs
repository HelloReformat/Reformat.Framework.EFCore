using Reformat.Framework.Core.Generator;

namespace Reformat.Data.EFCore.Annotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SnowflakeIdAttribute: Attribute
{
    public long GenerateId() => SnowflakeIdGenerator.GenerateId();
}