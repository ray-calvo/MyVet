using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyVet.Web.Data;
using MyVet.Web.Data.Entities;
using MyVet.Web.Helpers;
using MyVet.Web.Models;

namespace MyVet.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManagersController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public ManagersController(
            DataContext dataContext,
            IUserHelper userHelper,
            IMailHelper mailHelper)
        {
            _dataContext = dataContext;
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }

        public IActionResult Index()
        {
            return View(_dataContext.Managers.Include(m => m.User));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await _dataContext.Managers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id.Value);
            if (manager == null)
            {
                return NotFound();
            }

            return View(manager);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUserViewModel view)
        {
            if (ModelState.IsValid)
            {
                var user = await AddUser(view);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "This email is already used.");
                    return View(view);
                }

                var manager = new Manager { User = user };

                _dataContext.Managers.Add(manager);
                await _dataContext.SaveChangesAsync();

                var myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                var tokenLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userid = user.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                //_mailHelper.SendMail(view.Username, "Email confirmation", $"<h1>Email Confirmation</h1>" +
                //    $"To allow the user, " +
                //    $"plase click in this link:</br></br><a href = \"{tokenLink}\">Confirm Email</a>");
                _mailHelper.SendMail(view.Username, "Email confirmation",
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
                    $"    To allow the user,plase click in this link:</br></br> " +
                    $"    <a style ='text-decoration: none; border-radius: 5px; padding: 11px 23px; color: white; background-color: #3498db' href = \"{tokenLink}\">Confirm Email</a>" +
                    $"    <p style = 'color: #b3b3b3; font-size: 12px; text-align: center;margin: 30px 0 0' > Nuskë Clinica Integral Veterinaria 2019 </p>" +
                    $"  </div>" +
                    $" </td >" +
                    $"</tr>" +
                    $"</table>");

                return RedirectToAction(nameof(Index));
            }

            return View(view);
        }

        private async Task<User> AddUser(AddUserViewModel view)
        {
            var user = new User
            {
                Address = view.Address,
                Document = view.Document,
                Email = view.Username,
                FirstName = view.FirstName,
                LastName = view.LastName,
                PhoneNumber = view.PhoneNumber,
                UserName = view.Username
            };

            var result = await _userHelper.AddUserAsync(user, view.Password);
            if (result != IdentityResult.Success)
            {
                return null;
            }

            var newUser = await _userHelper.GetUserByEmailAsync(view.Username);
            await _userHelper.AddUserToRoleAsync(newUser, "Admin");
            return newUser;
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await _dataContext.Managers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (manager == null)
            {
                return NotFound();
            }

            var view = new EditUserViewModel
            {
                Address = manager.User.Address,
                Document = manager.User.Document,
                FirstName = manager.User.FirstName,
                Id = manager.Id,
                LastName = manager.User.LastName,
                PhoneNumber = manager.User.PhoneNumber
            };

            return View(view);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel view)
        {
            if (ModelState.IsValid)
            {
                var owner = await _dataContext.Owners
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == view.Id);

                owner.User.Document = view.Document;
                owner.User.FirstName = view.FirstName;
                owner.User.LastName = view.LastName;
                owner.User.Address = view.Address;
                owner.User.PhoneNumber = view.PhoneNumber;

                await _userHelper.UpdateUserAsync(owner.User);
                return RedirectToAction(nameof(Index));
            }

            return View(view);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = await _dataContext.Managers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (manager == null)
            {
                return NotFound();
            }

            _dataContext.Managers.Remove(manager);
            await _dataContext.SaveChangesAsync();
            await _userHelper.DeleteUserAsync(manager.User.Email);
            return RedirectToAction(nameof(Index));
        }

        private bool ManagerExists(int id)
        {
            return _dataContext.Managers.Any(e => e.Id == id);
        }
    }
}
