using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reformat.Data.EFCore.Annotations;
using Reformat.Framework.Core.Swagger.Annotation;

namespace Reformat.Data.EFCore.Core;

public abstract class EFCoreEntity
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [SnowflakeId]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity),Column("Id")]
    public long Id { get; set; }

    /// <summary>
    /// 逻辑删除
    /// </summary>
    [SwaggerIgnore]
    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;
}