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
                new Usuario { Id = 1, NombreUsuario = "admin", Contrasena = "admin123", NombreCompleto = "Administrador del Sistema", Rol = "Despachador" },
                new Usuario { Id = 2, NombreUsuario = "despacho", Contrasena = "despacho123", NombreCompleto = "Juan Pérez (Despachador)", Rol = "Despachador" },
                new Usuario { Id = 3, NombreUsuario = "piloto", Contrasena = "piloto123", NombreCompleto = "Martín Gómez (Comandante)", Rol = "Piloto" }
            );

            // Seed Aeronaves
            modelBuilder.Entity<Aeronave>().HasData(
                new Aeronave { Id = 1, Matricula = "LV-CUX", Modelo = "Boeing 737-800", CapacidadPasajeros = 170, Estado = "Activa" },
                new Aeronave { Id = 2, Matricula = "LV-FCR", Modelo = "Boeing 737-800 MAX", CapacidadPasajeros = 186, Estado = "Activa" },
                new Aeronave { Id = 3, Matricula = "LV-GKO", Modelo = "Embraer 190", CapacidadPasajeros = 96, Estado = "Activa" },
                new Aeronave { Id = 4, Matricula = "LV-FVL", Modelo = "Airbus A330-200", CapacidadPasajeros = 272, Estado = "Mantenimiento" }
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
                    AeronaveId = 4 
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
                    AeronaveId = 1 
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
                    AeronaveId = 2 
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
                    FechaCreacion = DateTime.Today.AddHours(6),
                    Observaciones = "Vuelo despachado con desvío estándar por clima favorable. Combustible incluye reserva regulada de 45 minutos."
                }
            );
        }
    }
}
