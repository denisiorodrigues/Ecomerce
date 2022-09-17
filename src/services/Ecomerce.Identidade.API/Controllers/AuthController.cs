using Ecomerce.Identidade.API.Extensions;
using Ecomerce.Identidade.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ecomerce.Identidade.API.Controllers
{
    [ApiController]
    [Route("api/identidade")]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSetting;

        public AuthController(SignInManager<IdentityUser> signInManager, 
                                UserManager<IdentityUser> userManager, 
                                IOptions<AppSettings> appSetting)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appSetting = appSetting.Value;
        }

        [HttpPost("nova-conta")]
        public async Task<IActionResult> Registrar(UsuarioRegistro usuarioRegistro)
        {
            if (!ModelState.IsValid) return BadRequest();

            var user = new IdentityUser()
            {
                UserName = usuarioRegistro.Email,
                Email = usuarioRegistro.Email,
                //Verificar na docuemntação do Identity sobre a confirmação de e-email
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, usuarioRegistro.Senha);

            if (result.Succeeded)
            {
                return CustomResponse(await GerarJwt(usuarioRegistro.Email));
            }

            foreach (var erro in result.Errors)
            {
                AdicionarErroProcessamento(erro.Description);
            }

            return CustomResponse();
        }

        [HttpPost("entrar")]
        public async Task<IActionResult> Login(UsuarioLogin usuarioLogin)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(usuarioLogin.Email, usuarioLogin.Senha, isPersistent:false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return CustomResponse(await GerarJwt(usuarioLogin.Email));
            }

            if (result.IsLockedOut)
            {
                AdicionarErroProcessamento("Usuário temporariamente bloqueado por tentativas inválidas");
                return CustomResponse();
            }

            AdicionarErroProcessamento("Usuário ou senha incorretos");
            return CustomResponse();
        }

        private async Task<UsuarioRespostaLogin> GerarJwt(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString())); // Quando o token vai expirar
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64)); // Quando o token foi emitido

            ///Adicionando as roles iguais as claims
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSetting.Secret);

            var token = tokenHandler.CreateToken(new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Issuer = _appSetting.Emissor,
                Audience = _appSetting.ValidadoEm,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSetting.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            });

            var encodedToken = tokenHandler.WriteToken(token);

            var response = new UsuarioRespostaLogin()
            { 
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSetting.ExpiracaoHoras).TotalSeconds,
                UsuarioToken = new UsuarioToken() 
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(x => new UsuarioClaim() { Type = x.Type, Value = x.Value })
                } 
            };
            
            return response;
        }

        private static long ToUnixEpochDate(DateTime date)
            => (long) Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970,1,1,0, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}
