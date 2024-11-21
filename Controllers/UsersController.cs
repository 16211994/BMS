using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BuildingManagment.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.AspNetCore.Http;

namespace BuildingManagment.Controllers
{
    public class UsersController : Controller
    {
        private readonly BIMSContext _context;

        public UsersController(BIMSContext context)
        {
            _context = context;
        }

        private string HashPassword(string Password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
        public IActionResult Register()
        {
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name");
           
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register([Bind("Id,FirstName,MiddleName,Password,Email,GenderId,PhoneNumber")] User user, string confirmPassword)
        {  
            if(user.Password != confirmPassword)
            {
                ModelState.AddModelError("Password", "Sorry,the password did't match!");
            }
            if(ModelState.IsValid)
            {
                user.Password = HashPassword(user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();
                
                TempData["sucess"] = "Registration successfull";
                
                
                return RedirectToAction("Login");
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", user.GenderId);
            return View();

        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            var hashedPassword = HashPassword(Password);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email && u.Password == hashedPassword);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt!");
                return View();
            }
            string fullName = $"{user.FirstName} {user.MiddleName}";
            HttpContext.Session.SetInt32("SenderId", user.Id);
            HttpContext.Session.SetString("FullName", fullName);
            HttpContext.Session.SetInt32("userid", user.Id);
            
            TempData["success"] = "Login successful!";
            return RedirectToAction("Index", "Home");
               
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var bIMSContext = _context.Users.Include(u => u.Gender);
            ViewData["userCount"] = bIMSContext.Count();
            //HttpContext.Session.SetString("count", ViewData["V"].ToString());
            return View(await bIMSContext.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Gender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name");
            
            return View();


        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GenderId,FirstName,MiddleName,Password,Email,PhoneNumber,CreatedDate,IsActive,IsDeleted")] User user)
        {
            if (user.FirstName == null)
            { 
                TempData["error"] = "Not Successed!";
                ModelState.AddModelError("FirstName", "required");
            }
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["success"] = "Successed!";
                return RedirectToAction(nameof(Index));
                

            }
            
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", user.GenderId);
            
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", user.GenderId);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,MiddleName,Password,Email,GenderId,PhoneNumber,CreatedDate,IsActive,IsDeleted")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user); 
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Successed!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", user.GenderId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Gender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
