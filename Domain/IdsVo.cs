using System.ComponentModel.DataAnnotations;

namespace Reformat.Data.EFCore.Domain;

public class IdsVo
{
    [Required]
    public List<long> Ids { get; set; }
}