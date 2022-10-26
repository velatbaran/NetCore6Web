using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCore6Web.Helpers;
using NetCore6Web.Models;

namespace NetCore6Web.Controllers
{
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]/[action]")]
    //[Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ITokenHelper _tokenHelper;

        public TestController(ITokenHelper tokenHelper)
        {
            _tokenHelper = tokenHelper;
        }

        [HttpGet]
        public IActionResult GetData()
        {

            return Ok(new {Name="Welat",Surname="BARAN"});
        }

        [HttpPost]
        public IActionResult PostData([FromBody] PostDataApiModel model)
        {
            return Ok(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody]LoginViewModel model)
        {
            if (model.Username == "welat" && model.Password == "212121")
            {
                string token = _tokenHelper.GenericToken(model.Username, 0);
                return Ok(new {Error = false,Message = "Token created",Data=token});
            }
            else
            {
                return BadRequest(new { Error = true, Message = "Username or password incorrect" });
            }
            
        }
    }
}
