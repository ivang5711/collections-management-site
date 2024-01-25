using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Collections.Controllers;

[Route("/[controller]")]
[ApiController]
public class StyleController : ControllerBase
{
    public Task<ActionResult> ChangeStyle([FromQuery] string style)
    {
        Response.Cookies.Append("style", style);
        return Task.FromResult<ActionResult>(Redirect("/"));
    }
}