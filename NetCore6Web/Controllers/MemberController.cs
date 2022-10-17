using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NETCore.Encrypt.Extensions;
using NetCore6Web.Entities;
using NetCore6Web.Models;
using System.Data;

namespace NetCore6Web.Controllers
{
    [Authorize(Roles = "admin")]
    public class MemberController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public MemberController(DatabaseContext databaseContext, IMapper mapper, IConfiguration configuration)
        {
            _databaseContext = databaseContext;
            _mapper = mapper;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MemberListPartial()
        {
            List<UserModel> users =
                _databaseContext.Users.ToList().Where(x=>x.IsRemoved == false)
                    .Select(x => _mapper.Map<UserModel>(x)).ToList();

            return PartialView("_MemberListPartial", users);
        }

        private string DoMD5HashedString(string password)
        {
            string md5Salt = _configuration.GetValue<string>("AppSettings:MD5Salt");
            string salted = password + md5Salt;
            string hashed = salted.MD5();
            return hashed;
        }

        public IActionResult AddNewUserPartial()
        {
            return PartialView("_AddNewUserPartial", new CreateUserModel());
        }

        [HttpPost]
        public IActionResult AddNewUser(CreateUserModel model)
        {
            if (ModelState.IsValid)
            {
                if (_databaseContext.Users.Any(x => x.Username.ToLower() == model.Username.ToLower() && x.IsRemoved == false))
                {
                    ModelState.AddModelError(nameof(model.Username), "Username is already exists.");
                    return PartialView("_AddNewUserPartial", model);
                }

                model.Password = DoMD5HashedString(model.Password);
                User user = _mapper.Map<User>(model);

                _databaseContext.Users.Add(user);
                _databaseContext.SaveChanges();

                return PartialView("_AddNewUserPartial", new CreateUserModel { Done = "User added." });
            }

            return PartialView("_AddNewUserPartial", model);
        }

        public IActionResult EditUserPartial(Guid id)
        {
            User user = _databaseContext.Users.Find(id);
            EditUserModel model = _mapper.Map<EditUserModel>(user);

            return PartialView("_EditUserPartial", model);
        }

        [HttpPost]
        public IActionResult EditUser(Guid id, EditUserModel model)
        {
            if (ModelState.IsValid)
            {
                if (_databaseContext.Users.Any(x => x.Username.ToLower() == model.Username.ToLower() && x.Id != id && x.IsRemoved == false))
                {
                    ModelState.AddModelError(nameof(model.Username), "Username is already exists.");
                    return PartialView("_EditUserPartial", model);
                }

                User user = _databaseContext.Users.Find(id);

                _mapper.Map(model, user);
                _databaseContext.SaveChanges();

                return PartialView("_EditUserPartial", new EditUserModel { Done = "User updated." });
            }

            return PartialView("_EditUserPartial", model);
        }

        public IActionResult DeleteUser(Guid id)
        {
            User user = _databaseContext.Users.Find(id);

            if (user != null)
            {
                user.IsRemoved = true;
                user.RemovedAt = DateTime.Now;
                _databaseContext.SaveChanges();
                TempData["result"] = "User removed";
            }

            return MemberListPartial();
        }

        // Method 1
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

        // Method 2
        [HttpPost]
        public IActionResult GetLocked(Guid[] ids)
        {
            List<Guid> lockedUsers = _databaseContext.Users.Where(x => x.IsLocked == true && ids.Contains(x.Id)).Select(x => x.Id).ToList();
            return Json(new { result = lockedUsers });
        }

        [HttpPost]
        public IActionResult SetLockedOrUnLocked(Guid id , bool isLocked)
        {
            User user = _databaseContext.Users.Find(id);
            if (user == null)
            {
                return Json(new { hasError = true, Message = "User not is found" });
            }
            user.IsLocked = isLocked;
            _databaseContext.SaveChanges();
            return Json(new { hasError = false });
        }
    }
}
