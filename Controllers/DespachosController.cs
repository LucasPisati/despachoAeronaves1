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
    public class DespachosController : Controller
    {
        private readonly EscuelaDatabaseContext _context;

        public DespachosController(EscuelaDatabaseContext context)
        {
            _context = context;
        }

        // GET: Despachos
        public async Task<IActionResult> Index()
        {
            var dispatches = await _context.Despachos
                .Include(d => d.Vuelo)
                .ThenInclude(v => v!.Aeronave)
                .ToListAsync();
            return View(dispatches);
        }

        // GET: Despachos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var despacho = await _context.Despachos
                .Include(d => d.Vuelo)
                .ThenInclude(v => v!.Aeronave)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (despacho == null) return NotFound();

            return View(despacho);
        }

        // GET: Despachos/VerifyDispatch?flightId=5
        public async Task<IActionResult> VerifyDispatch(int flightId)
        {
            var despacho = await _context.Despachos
                .FirstOrDefaultAsync(d => d.VueloId == flightId);

            if (despacho != null)
            {
                return RedirectToAction(nameof(Details), new { id = despacho.Id });
            }

            return RedirectToAction(nameof(Create), new { flightId = flightId });
        }

        // GET: Despachos/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? flightId)
        {
            // Only show flights that don't have a dispatch yet
            var dispatchedFlightIds = await _context.Despachos.Select(d => d.VueloId).ToListAsync();
            
            var undispatchedFlights = await _context.Vuelos
                .Where(v => !dispatchedFlightIds.Contains(v.Id) || v.Id == flightId)
                .ToListAsync();

            ViewBag.VueloId = new SelectList(undispatchedFlights, "Id", "NumeroVuelo", flightId);
            
            if (flightId.HasValue)
            {
                var flight = await _context.Vuelos.Include(v => v.Aeronave).FirstOrDefaultAsync(v => v.Id == flightId.Value);
                if (flight != null)
                {
                    ViewBag.SelectedFlight = flight;
                }
            }

            return View();
        }

        // POST: Despachos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VueloId,DespachadorNombre,CombustibleRequerido,CargaPago,Ruta,ClimaReporte,Observaciones")] Despacho despacho)
        {
            if (ModelState.IsValid)
            {
                despacho.FechaCreacion = DateTime.Now;
                _context.Add(despacho);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var dispatchedFlightIds = await _context.Despachos.Select(d => d.VueloId).ToListAsync();
            var undispatchedFlights = await _context.Vuelos
                .Where(v => !dispatchedFlightIds.Contains(v.Id) || v.Id == despacho.VueloId)
                .ToListAsync();

            ViewBag.VueloId = new SelectList(undispatchedFlights, "Id", "NumeroVuelo", despacho.VueloId);
            return View(despacho);
        }

        [HttpGet]
        public async Task<IActionResult> GetFlightDetailsJson(int id)
        {
            var flight = await _context.Vuelos
                .Include(v => v.Aeronave)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (flight == null) return Json(null);

            return Json(new {
                matricula = flight.Aeronave?.Matricula ?? "No Asignada",
                modelo = flight.Aeronave?.Modelo ?? "Desconocido",
                origen = flight.Origen,
                destino = flight.Destino,
                salida = flight.FechaHoraSalida.ToString("dd/MM/yyyy HH:mm")
            });
        }
    }
}
