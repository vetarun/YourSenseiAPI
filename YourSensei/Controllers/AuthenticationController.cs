using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using YourSensei.Service;
using YourSensei.Data;
using YourSensei.ViewModel;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Configuration;

namespace YourSensei.Controllers
{
    [Authorize]
    [RoutePrefix("Authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IAuthenticationService _service;

        public AuthenticationController(AuthenticationService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("SignUp")]
        public async Task<IHttpActionResult> SignUp(SignupInputViewModel obj)
        {

            var result = await _service.SignUp(obj);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<IHttpActionResult> Login(string UserName, string Password)
        {

            LoginResponse loginResponse = await _service.Login(UserName, Password);
            if (loginResponse.Code == 200)
            {
                string key = ConfigurationManager.AppSettings["SecretKey"].ToString();
                var issuer = ConfigurationManager.AppSettings["Issuer"].ToString();

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                //Create a List of Claims, Keep claims name short    
                var permClaims = new List<Claim>();
                permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                permClaims.Add(new Claim("UserId", loginResponse.UserId.ToString()));
                permClaims.Add(new Claim("Name", loginResponse.Name));
                permClaims.Add(new Claim("UserRole", loginResponse.UserRole.ToString()));
                permClaims.Add(new Claim("Email", loginResponse.Email));
                permClaims.Add(new Claim("CompanyId", loginResponse.CompanyId.ToString()));
                permClaims.Add(new Claim("Usertypeid", loginResponse.Usertypeid.ToString()));

                //Create Security Token object by giving required parameters    
                var token = new JwtSecurityToken(issuer, //Issure    
                                issuer,  //Audience    
                                permClaims,
                                expires: DateTime.Now.AddDays(1),
                                signingCredentials: credentials);
                loginResponse.Token = new JwtSecurityTokenHandler().WriteToken(token);
            }

           

            return Ok(loginResponse);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(string link, string password, string email, string oldpassword)
        {
            var result = await _service.ResetPassword(link,password,email,oldpassword);
            return Ok(result);

        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(string link, string password, string email, string oldpassword)
        {
            var result = await _service.ResetPassword(link, password, email, oldpassword);
            return Ok(result);

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(string email)
        {

            var result = await _service.ForgotPassword(email);
                return Ok(result);
            
        }

        [HttpGet]
        [Route("AllPendingApproval")]
        public async Task<IHttpActionResult> AllPendingApproval(bool approved, bool rejected)
        {
            var result = await _service.AllPendingApproval(approved,rejected);
            return Ok(result);

        }

        [HttpPost]
        [Route("AcceptRejectList")]
        public async Task<IHttpActionResult> AcceptRejectList(List<PendingApprovalViewModel> model)
        {
            var result = await _service.AcceptRejectList(model);
            return Ok(result);

        }
        [AllowAnonymous]
        [HttpPost]
        [Route("SendSupport")]
        public async Task<IHttpActionResult> SendSupport(TechSupportInputViewModel obj)
        {

            var result = await _service.SendSupport(obj);
            return Ok(result);
        }
        [HttpGet]
        [Route("GetPaymentCardList")]
        public async Task<IHttpActionResult> GetPaymentCardList(int id, string companyId, string userId)
        {
            var result = await _service.GetPaymentCardList(id,companyId,userId);
            return Ok(result);

        }

        [HttpPost]
        [Route("AddUpdateCardDetails")]
        public async Task<IHttpActionResult> AddUpdateCardDetails(CardDetailsInputViewModel input)
        {

            var result = await _service.AddUpdateCardDetails(input);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetUserDetails")]
        public async Task<IHttpActionResult> GetUserDetails(string companyId)
        {
            var result = await _service.GetUserDetails(companyId);
            return Ok(result);

        }
       
    }
}
