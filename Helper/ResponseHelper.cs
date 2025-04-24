using leapcert_back.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace leapcert_back.Helper;

public class ResponseHelper
{
    public static IActionResult HandleError(ControllerBase controller, IResponses response)
    {
        return response.StatusCode switch
        {
            400 => controller.BadRequest(response),
            401 => controller.Unauthorized(response),
            403 => controller.Forbid(),
            404 => controller.NotFound(response),
            500 => controller.StatusCode(StatusCodes.Status500InternalServerError, response),
            _ => controller.StatusCode(response.StatusCode, response)
        };
    }
}