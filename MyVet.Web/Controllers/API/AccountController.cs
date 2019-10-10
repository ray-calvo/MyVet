using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyVet.Common.Models;
using MyVet.Web.Data;
using MyVet.Web.Data.Entities;
using MyVet.Web.Helpers;

namespace MyVet.Web.Controllers.API
{
    [Route("api/[Controller]")]
    public class AccountController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public AccountController(
            DataContext dataContext,
            IUserHelper userHelper,
            IMailHelper mailHelper)
        {
            _dataContext = dataContext;
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }

        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] UserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Bad request"
                });
            }

            var user = await _userHelper.GetUserByEmailAsync(request.Email);
            if (user != null)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "This email is already registered."
                });
            }

            user = new User
            {
                Address = request.Address,
                Document = request.Document,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.Phone,
                UserName = request.Email
            };

            var result = await _userHelper.AddUserAsync(user, request.Password);
            if (result != IdentityResult.Success)
            {
                return BadRequest(result.Errors.FirstOrDefault().Description);
            }


            var userNew = await _userHelper.GetUserByEmailAsync(request.Email);
            await _userHelper.AddUserToRoleAsync(userNew, "Customer");
            _dataContext.Owners.Add(new Owner { User = userNew });
            await _dataContext.SaveChangesAsync();

            var myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            var tokenLink = Url.Action("ConfirmEmail", "Account", new
            {
                userid = user.Id,
                token = myToken
            }, protocol: HttpContext.Request.Scheme);

            _mailHelper.SendMail(request.Email, "Email confirmation",
                     $"<table style = 'max-width: 600px; padding: 10px; margin:0 auto; border-collapse: collapse;'>" +
                     $"  <tr>" +
                     $"    <td style = 'background-color: #34495e; text-align: center; padding: 0'>" +
                     $"       <a href = 'https://www.facebook.com/NuskeCIV/' >" +
                     $"         <img width = '20%' style = 'display:block; margin: 1.5% 3%' src= 'https://veterinarianuske.com/wp-content/uploads/2016/10/line_separator.png'>" +
                     $"       </a>" +
                     $"  </td>" +
                     $"  </tr>" +
                     $"  <tr>" +
                     $"  <td style = 'padding: 0'>" +
                     $"     <img style = 'padding: 0; display: block' src = 'https://veterinarianuske.com/wp-content/uploads/2018/07/logo-nnske-blanck.jpg' width = '100%'>" +
                     $"  </td>" +
                     $"</tr>" +
                     $"<tr>" +
                     $" <td style = 'background-color: #ecf0f1'>" +
                     $"      <div style = 'color: #34495e; margin: 4% 10% 2%; text-align: justify;font-family: sans-serif'>" +
                     $"            <h1 style = 'color: #e67e22; margin: 0 0 7px' > Hola </h1>" +
                     $"                    <p style = 'margin: 2px; font-size: 15px'>" +
                     $"                      El mejor Hospital Veterinario Especializado de la Ciudad de Morelia enfocado a brindar servicios médicos y quirúrgicos<br>" +
                     $"                      aplicando las técnicas más actuales y equipo de vanguardia para diagnósticos precisos y tratamientos oportunos..<br>" +
                     $"                      Entre los servicios tenemos:</p>" +
                     $"      <ul style = 'font-size: 15px;  margin: 10px 0'>" +
                     $"        <li> Urgencias.</li>" +
                     $"        <li> Medicina Interna.</li>" +
                     $"        <li> Imagenologia.</li>" +
                     $"        <li> Pruebas de laboratorio y gabinete.</li>" +
                     $"        <li> Estetica canina.</li>" +
                     $"      </ul>" +
                     $"  <div style = 'width: 100%;margin:20px 0; display: inline-block;text-align: center'>" +
                     $"    <img style = 'padding: 0; width: 200px; margin: 5px' src = 'https://veterinarianuske.com/wp-content/uploads/2018/07/tarjetas.png'>" +
                     // $"    <img style = 'padding: 0; width: 200px; margin: 5px' src = 'https://veterinarianuske.com/wp-content/uploads/2016/10/line_separator.png'>" +
                     $"  </div>" +
                     $"  <div style = 'width: 100%; text-align: center'>" +
                     $"    <h2 style = 'color: #e67e22; margin: 0 0 7px' >Email Confirmation </h2>" +
                     $"    To allow the user,plase click in this link:</ br ></ br > " +
                     $"    <a style ='text-decoration: none; border-radius: 5px; padding: 11px 23px; color: white; background-color: #3498db' href = \"{tokenLink}\">Confirm Email</a>" +
                     $"    <p style = 'color: #b3b3b3; font-size: 12px; text-align: center;margin: 30px 0 0' > Nuskë Clinica Integral Veterinaria 2019 </p>" +
                     $"  </div>" +
                     $" </td >" +
                     $"</tr>" +
                     $"</table>");

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Message = "A Confirmation email was sent. Please confirm your account and log into the App."
            });
        }

        [HttpPost]
        [Route("RecoverPassword")]
        public async Task<IActionResult> RecoverPassword([FromBody] EmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Bad request"
                });
            }

            var user = await _userHelper.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "This email is not assigned to any user."
                });
            }

            var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("ResetPassword", "Account", new { token = myToken }, protocol: HttpContext.Request.Scheme);
            _mailHelper.SendMail(request.Email, "Password Reset", $"<h1>Recover Password</h1>" +
                $"<table style = 'max-width: 600px; padding: 10px; margin:0 auto; border-collapse: collapse;'>" +
                     $"  <tr>" +
                     $"    <td style = 'background-color: #34495e; text-align: center; padding: 0'>" +
                     $"       <a href = 'https://www.facebook.com/NuskeCIV/' >" +
                     $"         <img width = '20%' style = 'display:block; margin: 1.5% 3%' src= 'https://veterinarianuske.com/wp-content/uploads/2016/10/line_separator.png'>" +
                     $"       </a>" +
                     $"  </td>" +
                     $"  </tr>" +
                     $"  <tr>" +
                     $"  <td style = 'padding: 0'>" +
                     $"     <img style = 'padding: 0; display: block' src = 'https://veterinarianuske.com/wp-content/uploads/2018/07/logo-nnske-blanck.jpg' width = '100%'>" +
                     $"  </td>" +
                     $"</tr>" +
                     $"<tr>" +
                     $" <td style = 'background-color: #ecf0f1'>" +
                     $"      <div style = 'color: #34495e; margin: 4% 10% 2%; text-align: justify;font-family: sans-serif'>" +
                     $"            <h1 style = 'color: #e67e22; margin: 0 0 7px' > Hola </h1>" +
                     $"                    <p style = 'margin: 2px; font-size: 15px'>" +
                     $"                      El mejor Hospital Veterinario Especializado de la Ciudad de Morelia enfocado a brindar servicios médicos y quirúrgicos<br>" +
                     $"                      aplicando las técnicas más actuales y equipo de vanguardia para diagnósticos precisos y tratamientos oportunos..<br>" +
                     $"                      Entre los servicios tenemos:</p>" +
                     $"      <ul style = 'font-size: 15px;  margin: 10px 0'>" +
                     $"        <li> Urgencias.</li>" +
                     $"        <li> Medicina Interna.</li>" +
                     $"        <li> Imagenologia.</li>" +
                     $"        <li> Pruebas de laboratorio y gabinete.</li>" +
                     $"        <li> Estetica canina.</li>" +
                     $"      </ul>" +
                     $"  <div style = 'width: 100%;margin:20px 0; display: inline-block;text-align: center'>" +
                     $"    <img style = 'padding: 0; width: 200px; margin: 5px' src = 'https://veterinarianuske.com/wp-content/uploads/2018/07/tarjetas.png'>" +
                     // $"    <img style = 'padding: 0; width: 200px; margin: 5px' src = 'https://veterinarianuske.com/wp-content/uploads/2016/10/line_separator.png'>" +
                     $"  </div>" +
                     $"  <div style = 'width: 100%; text-align: center'>" +
                     $"    <h2 style = 'color: #e67e22; margin: 0 0 7px' >Email Confirmation </h2>" +
                     $"    To allow the user,plase click in this link:</ br ></ br > " +
                     $"    <a style ='text-decoration: none; border-radius: 5px; padding: 11px 23px; color: white; background-color: #3498db' href = \"{link}\">Confirm Email</a>" +
                     $"    <p style = 'color: #b3b3b3; font-size: 12px; text-align: center;margin: 30px 0 0' > Nuskë Clinica Integral Veterinaria 2019 </p>" +
                     $"  </div>" +
                     $" </td >" +
                     $"</tr>" +
                     $"</table>");

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Message = "An email with instructions to change the password was sent."
            });
        }
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutUser([FromBody] UserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userEntity = await _userHelper.GetUserByEmailAsync(request.Email);
            if (userEntity == null)
            {
                return BadRequest("User not found.");
            }

            userEntity.FirstName = request.FirstName;
            userEntity.LastName = request.LastName;
            userEntity.Address = request.Address;
            userEntity.PhoneNumber = request.Phone;
            userEntity.Document = request.Phone;

            var respose = await _userHelper.UpdateUserAsync(userEntity);
            if (!respose.Succeeded)
            {
                return BadRequest(respose.Errors.FirstOrDefault().Description);
            }

            var updatedUser = await _userHelper.GetUserByEmailAsync(request.Email);
            return Ok(updatedUser);
        }

        [HttpPost]
        [Route("ChangePassword")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Bad request"
                });
            }

            var user = await _userHelper.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "This email is not assigned to any user."
                });
            }

            var result = await _userHelper.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = result.Errors.FirstOrDefault().Description
                });
            }

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Message = "The password was changed successfully!"
            });
        }


    }

}
