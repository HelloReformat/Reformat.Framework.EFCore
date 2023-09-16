using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Reformat.Framework.Core.Generator;

namespace Reformat.Data.EFCore.Generator;

public class EntitySnowIdGenerator : ValueGenerator<long>
{
    public override long Next(EntityEntry entry)
    {
        return SnowflakeIdGenerator.GenerateId();
    }

    public override bool GeneratesTemporaryValues { get; }
}