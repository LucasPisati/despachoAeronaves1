using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using despachoAeronave.Models;
using System.Threading.Tasks;

namespace despachoAeronave.Controllers
{
    [Authorize]
    public class AeronavesController : Controller
    {
        private readonly EscuelaDatabaseContext _context;

        public AeronavesController(EscuelaDatabaseContext context)
        {
            _context = context;
        }

        // GET: Aeronaves
        public async Task<IActionResult> Index()
        {
            var fleet = await _context.Aeronaves.ToListAsync();
            return View(fleet);
        }

        // GET: Aeronaves/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Aeronaves/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Matricula,Modelo,CapacidadPasajeros,Estado,PesoMaximoDespegue,PesoVacio")] Aeronave aeronave)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aeronave);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aeronave);
        }

        // GET: Aeronaves/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var aeronave = await _context.Aeronaves.FindAsync(id);
            if (aeronave == null) return NotFound();

            return View(aeronave);
        }

        // POST: Aeronaves/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Matricula,Modelo,CapacidadPasajeros,Estado,PesoMaximoDespegue,PesoVacio")] Aeronave aeronave)
        {
            if (id != aeronave.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aeronave);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AeronaveExists(aeronave.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(aeronave);
        }

        // GET: Aeronaves/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var aeronave = await _context.Aeronaves.FirstOrDefaultAsync(m => m.Id == id);
            if (aeronave == null) return NotFound();

            return View(aeronave);
        }

        // POST: Aeronaves/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aeronave = await _context.Aeronaves.FindAsync(id);
            if (aeronave != null)
            {
                _context.Aeronaves.Remove(aeronave);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AeronaveExists(int id)
        {
            return _context.Aeronaves.Any(e => e.Id == id);
        }
    }
}






