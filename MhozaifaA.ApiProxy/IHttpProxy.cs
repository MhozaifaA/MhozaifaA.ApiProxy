using Meteors;
using Microsoft.AspNetCore.Mvc;

namespace MhozaifaA.ApiProxy
{
    public interface IHttpProxy
    {
        Task<JsonResult> Call { get; }

        Task<OperationResult<byte>> _Call();
    }
}