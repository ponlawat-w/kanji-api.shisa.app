using Microsoft.AspNetCore.Mvc;

namespace ShisaKanjis.Controllers.V1 {
  [Route("api/v1/[controller]")]
  [ApiController]
  public class AliveController: BaseController {
    [HttpGet]
    public IActionResult AliveApi() {
      return OkResponse();
    }
  }
}