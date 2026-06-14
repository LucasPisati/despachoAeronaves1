using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using despachoAeronave.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

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
                .Include(d => d.Vuelo!.Piloto)
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
                .Include(d => d.Vuelo!.Piloto)
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
                var flight = await _context.Vuelos
                    .Include(v => v.Aeronave)
                    .Include(v => v.Piloto)
                    .FirstOrDefaultAsync(v => v.Id == flightId.Value);
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
        public async Task<IActionResult> Create([Bind("VueloId,DespachadorNombre,CombustibleRequerido,CargaPago,Ruta,ClimaReporte,NotamsReporte,Observaciones")] Despacho despacho)
        {
            // Validaciones de Seguridad Operativa
            var flight = await _context.Vuelos
                .Include(v => v.Aeronave)
                .FirstOrDefaultAsync(v => v.Id == despacho.VueloId);

            if (flight == null)
            {
                ModelState.AddModelError("", "El vuelo seleccionado no existe.");
            }
            else if (flight.Aeronave == null)
            {
                ModelState.AddModelError("", "El vuelo seleccionado no tiene una aeronave asignada.");
            }
            else
            {
                // 1. Validar que la aeronave no esté en mantenimiento
                if (flight.Aeronave.Estado != "Activa")
                {
                    ModelState.AddModelError("", $"La aeronave {flight.Aeronave.Matricula} se encuentra en estado '{flight.Aeronave.Estado}' y no está disponible para volar.");
                }

                // 2. Validar sobrepeso y balanceo (Weight & Balance)
                double totalWeight = flight.Aeronave.PesoVacio + despacho.CargaPago + despacho.CombustibleRequerido;
                if (totalWeight > flight.Aeronave.PesoMaximoDespegue)
                {
                    ModelState.AddModelError("", $"Alerta de Sobrepeso: El peso calculado al despegue ({totalWeight:N0} kg) excede el Peso Máximo de Despegue (MTOW) permitido para la aeronave ({flight.Aeronave.PesoMaximoDespegue:N0} kg). Por favor, reduzca el peso de la carga o combustible.");
                }

                double rawMac = 22.0 + (despacho.CargaPago / 1000.0) * 0.35 - (despacho.CombustibleRequerido / 1000.0) * 0.12;
                if (rawMac < 15.0 || rawMac > 35.0)
                {
                    ModelState.AddModelError("", $"Alerta de Desbalanceo: El Centro de Gravedad calculado ({rawMac:F1}% MAC) se encuentra fuera del rango de seguridad certificado (15% - 35% MAC) para esta aeronave. Por favor, reduzca o redistribuya el peso de la carga y el combustible para balancear el avión.");
                }

                // 3. Validar si el aeropuerto de destino está cerrado por NOTAM
                if (!string.IsNullOrEmpty(despacho.NotamsReporte) && !string.IsNullOrEmpty(flight.Destino))
                {
                    string destIata = parseIataCode(flight.Destino);
                    string destIcao = mapIataToIcao(destIata);
                    
                    bool isClosed = false;
                    if (!string.IsNullOrEmpty(destIcao))
                    {
                        if (despacho.NotamsReporte.Contains($"{destIcao} AD CLSD", StringComparison.OrdinalIgnoreCase) || 
                            despacho.NotamsReporte.Contains($"{destIcao} RWY CLOSED", StringComparison.OrdinalIgnoreCase) ||
                            despacho.NotamsReporte.Contains($"{destIcao} AEROPUERTO CERRADO", StringComparison.OrdinalIgnoreCase))
                        {
                            isClosed = true;
                        }
                    }
                    
                    if (isClosed)
                    {
                        ModelState.AddModelError("", $"Despacho Rechazado: El aeropuerto de destino {flight.Destino} se encuentra CERRADO temporalmente según reporte NOTAM de seguridad operacional.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                despacho.FechaCreacion = DateTime.Now;
                despacho.EstaAprobadoPorPiloto = false;
                despacho.FechaFirmaPiloto = null;
                
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

        // POST: Despachos/Firmar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Firmar(int id)
        {
            var despacho = await _context.Despachos.FindAsync(id);
            if (despacho == null) return NotFound();

            despacho.EstaAprobadoPorPiloto = true;
            despacho.FechaFirmaPiloto = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = despacho.Id });
        }

        // GET: Despachos/GetRealMetar
        [HttpGet]
        public async Task<IActionResult> GetRealMetar(string icao)
        {
            if (string.IsNullOrEmpty(icao) || icao.Length != 4)
            {
                return Json(new { success = false, message = "Código ICAO inválido" });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var url = $"https://tgftp.nws.noaa.gov/data/observations/metar/stations/{icao.ToUpper()}.txt";
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; DespachoAeroApp/1.0)");
                    var response = await client.GetStringAsync(url);
                    
                    var lines = response.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length >= 2)
                    {
                        return Json(new { success = true, metar = lines[1].Trim() });
                    }
                    else if (lines.Length == 1)
                    {
                        return Json(new { success = true, metar = lines[0].Trim() });
                    }
                }
            }
            catch (Exception)
            {
                // Fallback handled on client side
            }

            return Json(new { success = false, message = "No se pudo obtener el reporte de clima real." });
        }

        [HttpGet]
        public async Task<IActionResult> GetFlightDetailsJson(int id)
        {
            var flight = await _context.Vuelos
                .Include(v => v.Aeronave)
                .Include(v => v.Piloto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (flight == null) return Json(null);

            return Json(new {
                matricula = flight.Aeronave?.Matricula ?? "No Asignada",
                modelo = flight.Aeronave?.Modelo ?? "Desconocido",
                origen = flight.Origen,
                destino = flight.Destino,
                salida = flight.FechaHoraSalida.ToString("dd/MM/yyyy HH:mm"),
                pesoVacio = flight.Aeronave?.PesoVacio ?? 0,
                pesoMaximoDespegue = flight.Aeronave?.PesoMaximoDespegue ?? 0,
                piloto = flight.Piloto?.NombreCompleto ?? "No Asignado"
            });
        }

        // Helpers
        private string parseIataCode(string airportText)
        {
            if (string.IsNullOrEmpty(airportText)) return "";
            var match = System.Text.RegularExpressions.Regex.Match(airportText, @"\b([A-Z]{3})\b");
            return match.Success ? match.Groups[1].Value : "";
        }

        private string mapIataToIcao(string iata)
        {
            switch (iata.ToUpper())
            {
                case "EZE": return "SAEZ";
                case "AEP": return "SABE";
                case "COR": return "SACO";
                case "FTE": return "SAWC";
                case "MAD": return "LEMD";
                default: return "";
            }
        }
    }
}








