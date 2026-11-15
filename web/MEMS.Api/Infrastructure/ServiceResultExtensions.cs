using MEMS.Data;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Infrastructure;

public static class ServiceResultExtensions
{
    // Maps a service outcome to an HTTP response. Failures use RFC 7807 problem details and never include internals.
    public static IActionResult ToActionResult<T>(this ServiceResult<T> result, ControllerBase controller) =>
        result.Status switch
        {
            ResultStatus.Ok => controller.Ok(result.Value),
            ResultStatus.Created => controller.StatusCode(StatusCodes.Status201Created, result.Value),
            _ => ((ServiceResult)result).ToActionResult(controller)
        };

    public static IActionResult ToActionResult(this ServiceResult result, ControllerBase controller) =>
        result.Status switch
        {
            ResultStatus.Ok or ResultStatus.Created => controller.NoContent(),
            ResultStatus.NotFound => controller.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found", detail: result.Error),
            ResultStatus.Forbidden => controller.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Forbidden", detail: result.Error),
            ResultStatus.Conflict => controller.Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflict", detail: result.Error),
            _ => controller.ValidationProblem(new ValidationProblemDetails(result.Errors ?? new Dictionary<string, string[]>()))
        };
}
