using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using NETCore.Encrypt.Extensions;
using NetCore6Web.Entities;
using NetCore6Web.Helpers;
using NetCore6Web.Models;
using System.Net;

namespace NetCore6Web.Controllers
{
    [Authorize(Roles = "admin", AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class UserController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHasher _hasher;

        public UserController(DatabaseContext databaseContext, IMapper mapper, IConfiguration configuration, IHasher hasher)
        {
            _databaseContext = databaseContext;
            _mapper = mapper;
            _configuration = configuration;
            _hasher = hasher;
        }

        public IActionResult Index()
        {
            List<UserModel> users =
                _databaseContext.Users.ToList().Where(x=>x.IsRemoved == false)
                    .Select(x => _mapper.Map<UserModel>(x)).ToList();

            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Create(CreateUserModel model)
        {
            if (ModelState.IsValid)
            {
                if (_databaseContext.Users.Any(x => x.Username.ToLower() == model.Username.ToLower() && x.IsRemoved == false))
                {
                    ModelState.AddModelError(nameof(model.Username), "Username is already exists.");
                    return View(model);
                }

                User user = _mapper.Map<User>(model);
                user.Password = _hasher.DoMD5HashedString(model.Password);

                _databaseContext.Users.Add(user);
                _databaseContext.SaveChanges();
                TempData["result"] = "User created";

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public IActionResult Edit(Guid id)
        {
            User user = _databaseContext.Users.Find(id);
            EditUserModel model = _mapper.Map<EditUserModel>(user);

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Edit(Guid id, EditUserModel model)
        {
            if (ModelState.IsValid)
            {
                if (_databaseContext.Users.Any(x => x.Username.ToLower() == model.Username.ToLower() && x.Id != id && x.IsRemoved == false))
                {
                    ModelState.AddModelError(nameof(model.Username), "Username is already exists.");
                    return View(model);
                }

                User user = _databaseContext.Users.Find(id);

                _mapper.Map(model, user);
                _databaseContext.SaveChanges();
                TempData["result"] = "User updated";

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public IActionResult Delete(Guid id)
        {
            User user = _databaseContext.Users.Find(id);
            if (user != null)
            {
                //_databaseContext.Users.Remove(user);
                user.IsRemoved = true;
                user.RemovedAt = DateTime.Now;
                _databaseContext.SaveChanges();
                TempData["result"] = "User removed";
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult UserLockedOrOpenlock(Guid id)
        {
            User user = _databaseContext.Users.Find(id);
            if (user != null)
            {
                if (user.IsLocked)
                {
                    user.IsLocked = false;
                    TempData["result"] = "User is opened lock";
                }
                else
                {
                    user.IsLocked = true;
                    TempData["result"] = "User is locked";
                }
                   
                _databaseContext.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }
    }
}
