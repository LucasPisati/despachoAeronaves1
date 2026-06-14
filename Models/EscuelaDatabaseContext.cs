using Microsoft.EntityFrameworkCore;
using System;

namespace despachoAeronave.Models
{
    public class EscuelaDatabaseContext : DbContext
    {
        public EscuelaDatabaseContext(DbContextOptions<EscuelaDatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Aeronave> Aeronaves { get; set; } = null!;
        public DbSet<Vuelo> Vuelos { get; set; } = null!;
        public DbSet<Despacho> Despachos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Usuarios
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { Id = 1, NombreUsuario = "admin", Contrasena = "AdminSecure$2026!", NombreCompleto = "Administrador del Sistema", Rol = "Despachador" },
                new Usuario { Id = 2, NombreUsuario = "despacho", Contrasena = "DespachoSecure$2026!", NombreCompleto = "Juan Pérez (Despachador)", Rol = "Despachador" },
                new Usuario { Id = 3, NombreUsuario = "piloto", Contrasena = "PilotoSecure$2026!", NombreCompleto = "Martín Gómez (Comandante)", Rol = "Piloto" },
                new Usuario { Id = 4, NombreUsuario = "piloto2", Contrasena = "PilotoSecure$2026!", NombreCompleto = "Laura Fernández (Comandante)", Rol = "Piloto" },
                new Usuario { Id = 5, NombreUsuario = "piloto3", Contrasena = "PilotoSecure$2026!", NombreCompleto = "Carlos Rodríguez (Comandante)", Rol = "Piloto" },
                new Usuario { Id = 6, NombreUsuario = "piloto4", Contrasena = "PilotoSecure$2026!", NombreCompleto = "Patricia Sosa (Comandante)", Rol = "Piloto" },
                new Usuario { Id = 7, NombreUsuario = "piloto5", Contrasena = "PilotoSecure$2026!", NombreCompleto = "Alejandro Silva (Comandante)", Rol = "Piloto" }
            );

            // Seed Aeronaves
            modelBuilder.Entity<Aeronave>().HasData(
                new Aeronave { Id = 1, Matricula = "LV-CUX", Modelo = "Boeing 737-800", CapacidadPasajeros = 170, Estado = "Activa", PesoMaximoDespegue = 79016, PesoVacio = 41413 },
                new Aeronave { Id = 2, Matricula = "LV-FCR", Modelo = "Boeing 737-800 MAX", CapacidadPasajeros = 186, Estado = "Activa", PesoMaximoDespegue = 82190, PesoVacio = 45070 },
                new Aeronave { Id = 3, Matricula = "LV-GKO", Modelo = "Embraer 190", CapacidadPasajeros = 96, Estado = "Activa", PesoMaximoDespegue = 51800, PesoVacio = 28080 },
                new Aeronave { Id = 4, Matricula = "LV-FVL", Modelo = "Airbus A330-200", CapacidadPasajeros = 272, Estado = "Mantenimiento", PesoMaximoDespegue = 242000, PesoVacio = 119600 }
            );

            // Seed Vuelos
            modelBuilder.Entity<Vuelo>().HasData(
                new Vuelo 
                { 
                    Id = 1, 
                    NumeroVuelo = "AR1300", 
                    Origen = "EZE (Buenos Aires)", 
                    Destino = "MAD (Madrid)", 
                    FechaHoraSalida = DateTime.Today.AddHours(23).AddMinutes(55), 
                    FechaHoraLlegada = DateTime.Today.AddDays(1).AddHours(14).AddMinutes(10), 
                    Estado = "Programado", 
                    AeronaveId = 4,
                    PilotoId = 5 // Carlos Rodríguez
                },
                new Vuelo 
                { 
                    Id = 2, 
                    NumeroVuelo = "AR1420", 
                    Origen = "AEP (Buenos Aires)", 
                    Destino = "COR (Córdoba)", 
                    FechaHoraSalida = DateTime.Today.AddHours(8).AddMinutes(30), 
                    FechaHoraLlegada = DateTime.Today.AddHours(9).AddMinutes(50), 
                    Estado = "Programado", 
                    AeronaveId = 1,
                    PilotoId = 3 // Martín Gómez
                },
                new Vuelo 
                { 
                    Id = 3, 
                    NumeroVuelo = "AR1844", 
                    Origen = "AEP (Buenos Aires)", 
                    Destino = "FTE (El Calafate)", 
                    FechaHoraSalida = DateTime.Today.AddHours(10).AddMinutes(15), 
                    FechaHoraLlegada = DateTime.Today.AddHours(13).AddMinutes(35), 
                    Estado = "Programado", 
                    AeronaveId = 2,
                    PilotoId = 4 // Laura Fernández
                }
            );

            // Seed Despachos
            modelBuilder.Entity<Despacho>().HasData(
                new Despacho
                {
                    Id = 1,
                    VueloId = 2,
                    DespachadorNombre = "Juan Pérez (Despachador)",
                    CombustibleRequerido = 4500, // 4.5 tons
                    CargaPago = 12500, // 12.5 tons
                    Ruta = "AEP UT312 COR",
                    ClimaReporte = "SABE 010600Z 12008KT 9999 FEW030 18/14 Q1015; SACO 010600Z 09005KT 9999 SKC 15/10 Q1016",
                    NotamsReporte = "SABE (AEP): ILS CAT I RWY 13 OPERATIONAL. SACO (COR): ALL SERVICES NORMAL.",
                    FechaCreacion = DateTime.Today.AddHours(6),
                    Observaciones = "Vuelo despachado con desvío estándar por clima favorable. Combustible incluye reserva regulada de 45 minutos.",
                    EstaAprobadoPorPiloto = false,
                    FirmaPilotoBase64 = null
                }
            );
        }
    }
}






