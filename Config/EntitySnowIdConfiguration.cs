using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reformat.Data.EFCore.Core;
using Reformat.Data.EFCore.Generator;

namespace Reformat.Data.EFCore.Config;

public class EntitySnowIdConfiguration<T>: IEntityTypeConfiguration<T> where T : EFCoreEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(e => e.Id)
            .HasAnnotation("SnowflakeId", true)
            .HasValueGenerator<EntitySnowIdGenerator>();
    }
}