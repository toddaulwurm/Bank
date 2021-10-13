using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace Bank.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private MyContext _context;

        public HomeController(ILogger<HomeController> logger, MyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View("LoginReg");
        }

        [HttpPost("RegisterUser")]
        public IActionResult RegisterUser(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(_context.Users.Any(user => user.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in system!");
                    return View("LoginReg");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);

                _context.Add(newUser);
                _context.SaveChanges();

                HttpContext.Session.SetInt32("LoggedUserId", newUser.UserId);


                return RedirectToAction("Success");
            }
            return View("LoginReg");
        }


        [HttpPost("login")]
        public IActionResult Login(LoginUser checkMe)
        {
            if(ModelState.IsValid)
            {
                User userInDb = _context.Users.FirstOrDefault(use => use.Email == checkMe.LoginEmail);
                if(userInDb == null)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Login!");
                    return View("LoginReg");
                }

                PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();

                var result = hasher.VerifyHashedPassword(checkMe, userInDb.Password, checkMe.LoginPassword);

                if(result ==0)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Login!");
                    return View("LoginReg");
                }

                HttpContext.Session.SetInt32("LoggedUserId", userInDb.UserId);

                return RedirectToAction("Success");
            }
            return View("LoginReg");
        }

        [HttpGet("success")]
        public IActionResult Success()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            ViewBag.AllTransactrions = _context.Transactions
                .Where(wom => wom.UserId == loggedUserId)
                .ToList();
            ViewBag.AllTransactrions.Reverse();

            ViewBag.User = _context.Users.FirstOrDefault(use => use.UserId == loggedUserId);
            return View();
        }

        [HttpPost("/amount")]
        public IActionResult Amount(Transaction checkMe)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            ViewBag.User = _context.Users.FirstOrDefault(use => use.UserId == loggedUserId);

            if(ModelState.IsValid)
            {
                if(ViewBag.User.Balance + checkMe.Amount < 0)
                {
                    Console.WriteLine(ViewBag.User.Balance + checkMe.Amount);
                    ModelState.AddModelError("Amount", "Not Enough Money!!!!");
                    return View("Success");
                }
                else
                {
                    Console.WriteLine(ViewBag.User.Balance + checkMe.Amount);

                    User retrievedUser = _context.Users.FirstOrDefault(user => user.UserId == loggedUserId);

                    retrievedUser.Balance = ViewBag.User.Balance + checkMe.Amount;

                    _context.Add(checkMe);
                    _context.SaveChanges();

                    return RedirectToAction("Success");
                }
                
            }
            return RedirectToAction("Success");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
