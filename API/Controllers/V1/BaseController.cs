using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ShisaKanjis.Controllers.V1 {
  public class BaseController: ControllerBase {
    public IActionResult OkResponse(object data = null) {
      return Ok(ApiResponse.CreateSuccessful(data));
    }

    public IActionResult BadRequestResposne(string status = ErrorMessages.InvalidInput, object data = null) {
      return BadRequest(ApiResponse.CreateUnsuccessful(status, data));
    }

    public IActionResult NotFoundResponse(string status = ErrorMessages.NotFound, object data = null) {
      return NotFound(ApiResponse.CreateUnsuccessful(status, data));
    }

    public IActionResult InternalServerErrorResponse(string status = ErrorMessages.Error, object data = null) {
      return new ObjectResult(ApiResponse.CreateUnsuccessful(status, data)) {
        StatusCode = StatusCodes.Status500InternalServerError
      };
    }
  }
}