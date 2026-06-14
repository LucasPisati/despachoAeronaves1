using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using despachoAeronave.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace despachoAeronave.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly EscuelaDatabaseContext _context;

        public HomeController(EscuelaDatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var isPilot = User.IsInRole("Piloto");
            
            // Get logged in pilot user ID from claims
            var userIdClaim = User.FindFirst("UserId")?.Value;
            int? loggedInUserId = null;
            if (int.TryParse(userIdClaim, out var id))
            {
                loggedInUserId = id;
            }

            IQueryable<Vuelo> flightsQuery = _context.Vuelos;
            IQueryable<Despacho> dispatchesQuery = _context.Despachos;

            if (isPilot && loggedInUserId.HasValue)
            {
                flightsQuery = flightsQuery.Where(v => v.PilotoId == loggedInUserId.Value);
                dispatchesQuery = dispatchesQuery.Where(d => d.Vuelo!.PilotoId == loggedInUserId.Value);
            }

            ViewBag.TotalFlights = await flightsQuery.CountAsync();
            ViewBag.ActiveAircraft = await _context.Aeronaves.CountAsync(a => a.Estado == "Activa");
            ViewBag.TotalDispatches = await dispatchesQuery.CountAsync();

            ViewBag.DispatchedFlightIds = await _context.Despachos.Select(d => d.VueloId).ToListAsync();
            
            // Retrieve dispatches details to map signatures on the home view
            ViewBag.PilotSignedFlightIds = await _context.Despachos
                .Where(d => d.EstaAprobadoPorPiloto)
                .Select(d => d.VueloId)
                .ToListAsync();

            var recentFlights = await flightsQuery
                .Include(v => v.Aeronave)
                .Include(v => v.Piloto)
                .OrderBy(v => v.FechaHoraSalida)
                .Take(10)
                .ToListAsync();

            return View(recentFlights);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
