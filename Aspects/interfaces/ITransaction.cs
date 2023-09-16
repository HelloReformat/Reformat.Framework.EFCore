using Reformat.Data.EFCore.Core;

namespace Reformat.Data.EFCore.Aspects.interfaces;

/// <summary>
/// 事务处理接口
/// </summary>
public interface ITransaction
{
    public EFCoreDbContext DbContext { get; set; }
}