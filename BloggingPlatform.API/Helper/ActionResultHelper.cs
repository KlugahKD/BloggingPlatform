using BloggingPlatform.Business.Common;
using Microsoft.AspNetCore.Mvc;

namespace BloggingPlatform.API.Helper;

public static class ActionResultHelper
{
    public static IActionResult ToActionResult<T>(ServiceResponse<T> response)
    {
        return new ObjectResult(response)
        {
            StatusCode = response.Code
        };
    }
}