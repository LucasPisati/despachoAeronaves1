using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using despachoAeronave.Models;
using System.Linq;
using System.Threading.Tasks;

namespace despachoAeronave.Controllers
{
    [Authorize]
    public class VuelosController : Controller
    {
        private readonly EscuelaDatabaseContext _context;

        public VuelosController(EscuelaDatabaseContext context)
        {
            _context = context;
        }

        // GET: Vuelos
        public async Task<IActionResult> Index()
        {
            var flights = await _context.Vuelos.Include(v => v.Aeronave).ToListAsync();
            return View(flights);
        }

        // GET: Vuelos/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var activeAircraft = await _context.Aeronaves.Where(a => a.Estado == "Activa").ToListAsync();
            ViewBag.AeronaveId = new SelectList(activeAircraft, "Id", "Matricula");
            return View();
        }

        // POST: Vuelos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NumeroVuelo,Origen,Destino,FechaHoraSalida,FechaHoraLlegada,Estado,AeronaveId")] Vuelo vuelo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vuelo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var activeAircraft = await _context.Aeronaves.Where(a => a.Estado == "Activa").ToListAsync();
            ViewBag.AeronaveId = new SelectList(activeAircraft, "Id", "Matricula", vuelo.AeronaveId);
            return View(vuelo);
        }

        // GET: Vuelos/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vuelo = await _context.Vuelos.FindAsync(id);
            if (vuelo == null) return NotFound();

            var activeAircraft = await _context.Aeronaves.Where(a => a.Estado == "Activa" || a.Id == vuelo.AeronaveId).ToListAsync();
            ViewBag.AeronaveId = new SelectList(activeAircraft, "Id", "Matricula", vuelo.AeronaveId);
            return View(vuelo);
        }

        // POST: Vuelos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NumeroVuelo,Origen,Destino,FechaHoraSalida,FechaHoraLlegada,Estado,AeronaveId")] Vuelo vuelo)
        {
            if (id != vuelo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vuelo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VueloExists(vuelo.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var activeAircraft = await _context.Aeronaves.Where(a => a.Estado == "Activa" || a.Id == vuelo.AeronaveId).ToListAsync();
            ViewBag.AeronaveId = new SelectList(activeAircraft, "Id", "Matricula", vuelo.AeronaveId);
            return View(vuelo);
        }

        // GET: Vuelos/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vuelo = await _context.Vuelos.Include(v => v.Aeronave).FirstOrDefaultAsync(m => m.Id == id);
            if (vuelo == null) return NotFound();

            return View(vuelo);
        }

        // POST: Vuelos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vuelo = await _context.Vuelos.FindAsync(id);
            if (vuelo != null)
            {
                _context.Vuelos.Remove(vuelo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VueloExists(int id)
        {
            return _context.Vuelos.Any(e => e.Id == id);
        }
    }
}
