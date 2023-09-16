using Reformat.Data.EFCore.Enums;
namespace Reformat.Data.EFCore.Domain;

public class SqlQuery<T> 
{
    /// <summary>
    /// 查询条件
    /// </summary>
    public T QueryParms { get; set; }
    
    /// <summary>
    /// 分页大小 0的时候返回所有 (默认：20)
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 第几页（默认：1）
    /// </summary>
    public int PageIndex { get; set; } = 1;
    
    /// <summary>
    /// 排序参数：【字段名称 ：0（正序）/1（倒叙）】 
    /// </summary>
    public Dictionary<string,OrderType> OrderParms { get; set; }
}