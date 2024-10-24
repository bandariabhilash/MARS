using DataAccess.Db;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ServiceApis.IRepository;
using ServiceApis.ISecurity;
using ServiceApis.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ServiceApis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthRepository _authRepository;
        private readonly ISecurityClass _securityClass;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthRepository authRepository, ISecurityClass securityClass, IAuthService authService, IConfiguration configuration, ILogger<AuthController> logger, IWebHostEnvironment webHostEnvironment)
        {

            _authRepository = authRepository;
            _securityClass = securityClass;
            _authService = authService;
            _configuration = configuration;

        }


        [HttpPost]
        [Route("login")]

        public IActionResult Login([FromBody] LoginModel model)
        {
            FbUserMaster user = _authRepository.GetUser(model.Username);
            //if (user != null && user.Password == _securityClass.EncryptPwd(model.Password))
            if (user != null && user.Password ==model.Password)
            {

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.GivenName, user.FirstName + " " + user.LastName),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };


                var token = _authService.CreateToken(authClaims);
                var refreshToken = _authService.GenerateRefreshToken();

                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);
                UserRefreshtoken rtoken = new UserRefreshtoken();
                rtoken.Refreshtoken = refreshToken;
                rtoken.Userid = user.UserId;
                rtoken.Refreshtokenexpirytime = DateTime.Now.AddDays(refreshTokenValidityInDays);
                _authRepository.UpdateRefreshToken(rtoken);
                _authRepository.UpdateUser(user);
                //string userid = _securityClass.EncryptbyId(user.UserId);
                string userid = user.UserId.ToString();

                return Ok(new
                {
                    userid = userid,
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("refresh-token")]

        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest("Invalid client request");
            }

            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;

            var principal = _authService.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return BadRequest("Invalid access token or refresh token");
            }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            string username = principal.Identity.Name;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            var user = _authRepository.GetUser(username);
            //var rtoken = _authRepository.getWhuserRefreshtoken(refreshToken);

            if (user == null || /*rtoken == null || rtoken.Refreshtokenexpirytime <= DateTime.Now*/ user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            var newAccessToken = _authService.CreateToken(principal.Claims.ToList());
            var newRefreshToken = _authService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            _authRepository.UpdateUser(user);
            var newrtoken = new UserRefreshtoken();
            newrtoken.Refreshtoken = newRefreshToken;
            newrtoken.Userid = user.UserId;
            newrtoken.Refreshtokenexpirytime = (DateTime)user.RefreshTokenExpiryTime;
            _authRepository.UpdateRefreshToken(newrtoken);
            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }



    }
}
