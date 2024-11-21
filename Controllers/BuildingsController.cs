using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BuildingManagment.Models;

namespace BuildingManagment.Controllers
{
    public class BuildingsController : Controller
    {
        private readonly BIMSContext _context;

        public BuildingsController(BIMSContext context)
        {
            _context = context;
        }

        // GET: Buildings
        public async Task<IActionResult> Index()
        {
            var bIMSContext = _context.Buildings.Include(b => b.BuildingType).Include(b => b.City).Include(b => b.Location).Include(b => b.Owner).Include(b => b.OwnershipType).Include(b => b.User).Include(b => b.UseType);
            return View(await bIMSContext.ToListAsync());
        }

        // GET: Buildings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var building = await _context.Buildings
                .Include(b => b.BuildingType)
                .Include(b => b.City)
                .Include(b => b.Location)
                .Include(b => b.Owner)
                .Include(b => b.OwnershipType)
                .Include(b => b.User)
                .Include(b => b.UseType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (building == null)
            {
                return NotFound();
            }

            return View(building);
        }

        // GET: Buildings/Create
        public IActionResult Create()
        {
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Name");
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Name");
            ViewData["OwnershipTypeId"] = new SelectList(_context.OwnershipTypes, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Name");
            ViewData["UseTypeId"] = new SelectList(_context.UseTypes, "Id", "Name");
            return View();
        }

        // POST: Buildings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UseTypeId,UserId,CityId,LocationId,Name,Description,ConstractionYear,NumberOfFloor,BuildingTypeId,OwnershipTypeId,OwnerId,IsActive,IsDeleted")] Building building)
        {
            if (ModelState.IsValid)
            {
                _context.Add(building);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Name", building.BuildingTypeId);
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", building.CityId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", building.LocationId);
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Name", building.OwnerId);
            ViewData["OwnershipTypeId"] = new SelectList(_context.OwnershipTypes, "Id", "Name", building.OwnershipTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Name", building.UserId);
            ViewData["UseTypeId"] = new SelectList(_context.UseTypes, "Id", "Name", building.UseTypeId);
            return View(building);
        }

        // GET: Buildings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Name", building.BuildingTypeId);
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", building.CityId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", building.LocationId);
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "Name", building.OwnerId);
            ViewData["OwnershipTypeId"] = new SelectList(_context.OwnershipTypes, "Id", "Name", building.OwnershipTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Name", building.UserId);
            ViewData["UseTypeId"] = new SelectList(_context.UseTypes, "Id", "Name", building.UseTypeId);
            return View(building);
        }

        // POST: Buildings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UseTypeId,UserId,CityId,LocationId,Name,Description,ConstractionYear,NumberOfFloor,BuildingTypeId,OwnershipTypeId,OwnerId,IsActive,IsDeleted")] Building building)
        {
            if (id != building.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(building);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuildingExists(building.Id))
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
            ViewData["BuildingTypeId"] = new SelectList(_context.BuildingTypes, "Id", "Name", building.BuildingTypeId);
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", building.CityId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", building.LocationId);
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "name", building.OwnerId);
            ViewData["OwnershipTypeId"] = new SelectList(_context.OwnershipTypes, "Id", "Name", building.OwnershipTypeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Name", building.UserId);
            ViewData["UseTypeId"] = new SelectList(_context.UseTypes, "Id", "Name", building.UseTypeId);
            return View(building);
        }

        // GET: Buildings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var building = await _context.Buildings
                .Include(b => b.BuildingType)
                .Include(b => b.City)
                .Include(b => b.Location)
                .Include(b => b.Owner)
                .Include(b => b.OwnershipType)
                .Include(b => b.User)
                .Include(b => b.UseType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (building == null)
            {
                return NotFound();
            }

            return View(building);
        }

        // POST: Buildings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building != null)
            {
                _context.Buildings.Remove(building);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BuildingExists(int id)
        {
            return _context.Buildings.Any(e => e.Id == id);
        }
    }
}
