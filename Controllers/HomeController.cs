using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using despachoAeronave.Models;
using System.Linq;
using System.Threading.Tasks;

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
            ViewBag.TotalFlights = await _context.Vuelos.CountAsync();
            ViewBag.ActiveAircraft = await _context.Aeronaves.CountAsync(a => a.Estado == "Activa");
            ViewBag.TotalDispatches = await _context.Despachos.CountAsync();

            ViewBag.DispatchedFlightIds = await _context.Despachos.Select(d => d.VueloId).ToListAsync();

            var recentFlights = await _context.Vuelos
                .Include(v => v.Aeronave)
                .OrderBy(v => v.FechaHoraSalida)
                .Take(5)
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
