using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BuildingManagment.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Protocol.Plugins;

namespace BuildingManagment.Controllers
{
    public class ChatsController : Controller
    {
        private readonly BIMSContext _context;

        public ChatsController(BIMSContext context)
        {
            _context = context;
        }


        public JsonResult GetAllUsersWithChatSummary()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("userid");
                if (!userId.HasValue)
                {
                    return Json(new { success = false, error = "Session expired. Please log in again." });
                }

                var users = _context.Users
                    .Where(u => u.Id != userId) 
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        LastMessageDate = _context.Chats
                            .Where(c => (c.SenderId == userId && c.ReceiverId == u.Id) ||
                                        (c.SenderId == u.Id && c.ReceiverId == userId))
                            .OrderByDescending(c => c.Date)
                            .Select(c => c.Date)
                            .FirstOrDefault(),
                        UnreadMessagesCount = _context.Chats
                            .Count(c => c.SenderId == u.Id && c.ReceiverId == userId && c.ChatStatusId == 2) 
                    })
                    .OrderByDescending(u => u.LastMessageDate) 
                    .ToList();

                return Json(new { success = true, users });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user list with chat summary: {ex.Message}");
                return Json(new { success = false, error = "An error occurred while fetching users." });
            }
        }


        [HttpGet]
        public IActionResult GetUserChatHistory(int partnerId)
        {
            try
            {
                int? senderId = HttpContext.Session.GetInt32("userid");
                if (!senderId.HasValue)
                {
                    return Json(new { success = false, error = "Session expired. Please log in again." });
                }
              
                
                var chatHistory = _context.Chats
                    .Include(c => c.Parent)
                    .Where(c =>
                        (c.SenderId == senderId && c.ReceiverId == partnerId) ||
                        (c.SenderId == partnerId && c.ReceiverId == senderId))
                    .OrderBy(c => c.Date)
                    .ToList();

                var messagesToUpdate = chatHistory
                    .Where(c => c.ReceiverId == senderId.Value && c.ChatStatusId == 2)
                    .ToList();

                foreach (var message in messagesToUpdate)
                {
                    message.ChatStatusId = 1; 
                }

                _context.SaveChanges();

                var result = chatHistory.Select(c => new
                {
                    c.Message,
                    c.Date,
                    IsSentByMe = c.SenderId == senderId
                }).ToList();
                
                return Json(new { success = true, chatHistory = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching chat history: {ex.Message}");
                return Json(new { success = false, error = "An error occurred while fetching chat history." });
            }
        }



        [HttpPost]
        public JsonResult SendMessage(int receiverId, string message)
        {
     
            try
            {
                int? senderId = HttpContext.Session.GetInt32("SenderId");
                if (!senderId.HasValue)
                {
                    Console.WriteLine("SenderId is not set in session.");
                    return Json(new { success = false, error = "Session expired. Please log in again." });
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    return Json(new { success = false, error = "Message cannot be empty." });
                }

                var newChat = new Chat
                {
                    SenderId = senderId.Value,
                    ReceiverId = receiverId,
                    Message = message,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    IsActive = true,
                    IsDeleted = false,
                    ChatStatusId = 2 
                };

                _context.Chats.Add(newChat);
                _context.SaveChanges();

                 var unreadMessagesCount = _context.Chats
                .Where(c => c.ReceiverId == senderId.Value && c.ChatStatusId == 2 && c.IsActive && !c.IsDeleted)
                .Count();

            
            ViewData["userCount"] = unreadMessagesCount;

                ViewBag.SenderId = senderId;

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                return Json(new { success = false, error = "An error occurred while sending the message." });
            }
        }






        // GET: Chats
        public ActionResult Index(int receiverId, int? parentId)
        {
           
            var senderId = 1;  
            var chats = _context.Chats
                .Include(c => c.Sender)       // Ensure Sender is loaded
                .Include(c => c.Receiver)     // Ensure Receiver is loaded
                .Include(c => c.Parent)       // Ensure Parent is loaded for replies
                .Where(c => (c.SenderId == senderId && c.ReceiverId == receiverId) ||
                            (c.SenderId == receiverId && c.ReceiverId == senderId))
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Date)
                .ToList();

            // Pass the list of chats and the ParentId to the view
            ViewBag.ParentId = parentId;
            return View(chats);
        }

        [HttpPost]
        public ActionResult SendMessage1(int RecieiverId, string message,int? parentId)
        {
            Chat chat = new Chat();
            int userId = (int)HttpContext.Session.GetInt32("userid");
                chat.Date = DateOnly.FromDateTime(DateTime.Now);
                chat.IsActive = true;
                chat.IsDeleted = false;
                chat.ChatStatusId = 1; // Assume "1" means "Sent" status, update as needed.
                chat.SenderId = userId;
                chat.ReceiverId = RecieiverId;
                chat.ParentId = parentId;
            
                
                _context.Chats.Add(chat);
                _context.SaveChanges();

                // Redirect back to the chat with the receiver
                return RedirectToAction("Index", new { receiverId = chat.ReceiverId });
           

            // If model state is not valid, return the view to show validation errors
            return View("Index");
        }


        // GET: Chats/Details/5
        public async Task<IActionResult> Details(int? id)
           {
            if (id == null)
            {
                return NotFound();
            }

            var chat = await _context.Chats
                .Include(c => c.ChatStatus)
                .Include(c => c.Parent)
                .Include(c => c.Receiver)
                .Include(c => c.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chat == null)
            {
                return NotFound();
            }

            return View(chat);
        }

        // GET: Chats/Create
        public IActionResult Create()
        {
            ViewData["ChatStatusId"] = new SelectList(_context.ChatStatuses, "Id", "Name");
            ViewData["ParentId"] = new SelectList(_context.Chats, "Id", "Message");
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "FirstName");
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FirstName");
            return View();
        }

        // POST: Chats/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SenderId,ReceiverId,ParentId,Message,ChatStatusId,Date,IsActive,IsDeleted")] Chat chat)
        {
            
            if (ModelState.IsValid)
            {
                _context.Add(chat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ChatStatusId"] = new SelectList(_context.ChatStatuses, "Id", "Name", chat.ChatStatusId);
            ViewData["ParentId"] = new SelectList(_context.Chats, "Id", "Message", chat.ParentId);
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "FirstName", chat.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "FirstName", chat.SenderId);
            return View(chat);
        }

        // GET: Chats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {   
            if (id == null)
            {
                return NotFound();
            }

            var chat = await _context.Chats.FindAsync(id);
            if (chat == null)
            {
                return NotFound();
            }
            
            ViewData["ChatStatusId"] = new SelectList(_context.ChatStatuses, "Id", "Name", chat.ChatStatusId);
            ViewData["ParentId"] = new SelectList(_context.Chats, "Id", "Message", chat.ParentId);
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Email", chat.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Email", chat.SenderId);
            return View(chat);
        }

        // POST: Chats/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SenderId,ReceiverId,ParentId,Message,ChatStatusId,Date,IsActive,IsDeleted")] Chat chat)
        {
            if (id != chat.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChatExists(chat.Id))
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
            ViewData["ChatStatusId"] = new SelectList(_context.ChatStatuses, "Id", "Name", chat.ChatStatusId);
            ViewData["ParentId"] = new SelectList(_context.Chats, "Id", "Message", chat.ParentId);
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Email", chat.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Email", chat.SenderId);
            return View(chat);
        }

        // GET: Chats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chat = await _context.Chats
                .Include(c => c.ChatStatus)
                .Include(c => c.Parent)
                .Include(c => c.Receiver)
                .Include(c => c.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chat == null)
            {
                return NotFound();
            }

            return View(chat);
        }

        // POST: Chats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat != null)
            {
                _context.Chats.Remove(chat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChatExists(int id)
        {
            return _context.Chats.Any(e => e.Id == id);
        }
    }
}
