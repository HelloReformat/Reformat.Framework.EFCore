
using Reformat.Data.EFCore.Enums;

namespace Reformat.Data.EFCore.Annotations;

/// <summary>
/// 查询
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class WhereAttribute : Attribute
{
    public WhereAttribute()
    {
        this.type = ConditionalType.Equal;
    }
    
    public WhereAttribute(ConditionalType type)
    {
        this.type = type;
    }

    /// <summary>
    /// 关系
    /// </summary>
    public ConditionalType type { get; set; }
}