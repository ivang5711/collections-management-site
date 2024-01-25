using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Collections.Controllers;

[Route("/[controller]")]
[ApiController]
public class StyleController : ControllerBase
{
    public Task<ActionResult> ChangeStyle([FromQuery] string style, string redirectUri)
    {
        Response.Cookies.Append("style", style);
        return Task.FromResult<ActionResult>(Redirect(redirectUri));
    }
}