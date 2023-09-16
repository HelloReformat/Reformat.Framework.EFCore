using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Reformat.Framework.Core.Core;
using Reformat.Framework.Core.IOC.Attributes;
using Reformat.Framework.Core.IOC.Services;

namespace Reformat.Data.EFCore.Core;

[ApiController]
[Route("api/[controller]/[action]")]
public class EFCoreController: BaseController
{
    public EFCoreController(IocScoped iocScoped) : base(iocScoped)
    {
    }
}