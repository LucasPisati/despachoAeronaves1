using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using despachoAeronave.Models;
using System;
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
            var flights = await _context.Vuelos
                .Include(v => v.Aeronave)
                .Include(v => v.Piloto)
                .ToListAsync();
            return View(flights);
        }

        // GET: Vuelos/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var activeAircraft = await _context.Aeronaves.Where(a => a.Estado == "Activa").ToListAsync();
            ViewBag.AeronaveId = new SelectList(activeAircraft, "Id", "Matricula");
            
            var pilots = await _context.Usuarios.Where(u => u.Rol == "Piloto").ToListAsync();
            ViewBag.PilotoId = new SelectList(pilots, "Id", "NombreCompleto");
            return View();
        }

        // POST: Vuelos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NumeroVuelo,Origen,Destino,FechaHoraSalida,FechaHoraLlegada,Estado,AeronaveId,PilotoId")] Vuelo vuelo)
        {
            if (vuelo.FechaHoraSalida >= vuelo.FechaHoraLlegada)
            {
                ModelState.AddModelError("", "La fecha de salida debe ser anterior a la de llegada.");
            }
            else
            {
                var overlapExists = await _context.Vuelos.AnyAsync(f => 
                    f.Id != vuelo.Id && 
                    f.AeronaveId == vuelo.AeronaveId && 
                    f.Estado != "Cancelado" && 
                    f.Estado != "Aterrizado" && 
                    f.FechaHoraSalida < vuelo.FechaHoraLlegada && 
                    f.FechaHoraLlegada > vuelo.FechaHoraSalida);
                
                if (overlapExists)
                {
                    ModelState.AddModelError("", "Conflicto de flota: La aeronave seleccionada ya tiene un vuelo programado en ese rango de horario.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(vuelo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            var activeAircraft = await _context.Aeronaves.Where(a => a.Estado == "Activa").ToListAsync();
            ViewBag.AeronaveId = new SelectList(activeAircraft, "Id", "Matricula", vuelo.AeronaveId);
            
            var pilots = await _context.Usuarios.Where(u => u.Rol == "Piloto").ToListAsync();
            ViewBag.PilotoId = new SelectList(pilots, "Id", "NombreCompleto", vuelo.PilotoId);
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
            
            var pilots = await _context.Usuarios.Where(u => u.Rol == "Piloto").ToListAsync();
            ViewBag.PilotoId = new SelectList(pilots, "Id", "NombreCompleto", vuelo.PilotoId);
            return View(vuelo);
        }

        // POST: Vuelos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NumeroVuelo,Origen,Destino,FechaHoraSalida,FechaHoraLlegada,Estado,AeronaveId,PilotoId")] Vuelo vuelo)
        {
            if (id != vuelo.Id) return NotFound();

            if (vuelo.FechaHoraSalida >= vuelo.FechaHoraLlegada)
            {
                ModelState.AddModelError("", "La fecha de salida debe ser anterior a la de llegada.");
            }
            else
            {
                var overlapExists = await _context.Vuelos.AnyAsync(f => 
                    f.Id != vuelo.Id && 
                    f.AeronaveId == vuelo.AeronaveId && 
                    f.Estado != "Cancelado" && 
                    f.Estado != "Aterrizado" && 
                    f.FechaHoraSalida < vuelo.FechaHoraLlegada && 
                    f.FechaHoraLlegada > vuelo.FechaHoraSalida);
                
                if (overlapExists)
                {
                    ModelState.AddModelError("", "Conflicto de flota: La aeronave seleccionada ya tiene un vuelo programado en ese rango de horario.");
                }
            }

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
            
            var pilots = await _context.Usuarios.Where(u => u.Rol == "Piloto").ToListAsync();
            ViewBag.PilotoId = new SelectList(pilots, "Id", "NombreCompleto", vuelo.PilotoId);
            return View(vuelo);
        }

        // GET: Vuelos/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vuelo = await _context.Vuelos
                .Include(v => v.Aeronave)
                .Include(v => v.Piloto)
                .FirstOrDefaultAsync(m => m.Id == id);
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

        // POST: Vuelos/ActualizarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int id, string nuevoEstado)
        {
            var flight = await _context.Vuelos.FindAsync(id);
            if (flight == null) return NotFound();

            if (nuevoEstado == "Programado" || nuevoEstado == "En Vuelo" || nuevoEstado == "Aterrizado" || nuevoEstado == "Cancelado")
            {
                flight.Estado = nuevoEstado;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            
            return BadRequest("Estado de vuelo inválido");
        }

        private bool VueloExists(int id)
        {
            return _context.Vuelos.Any(e => e.Id == id);
        }
    }
}
