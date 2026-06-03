using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace despachoAeronave.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aeronaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Matricula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CapacidadPasajeros = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aeronaves", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Contrasena = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vuelos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroVuelo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Origen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaHoraSalida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraLlegada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AeronaveId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vuelos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vuelos_Aeronaves_AeronaveId",
                        column: x => x.AeronaveId,
                        principalTable: "Aeronaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Despachos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VueloId = table.Column<int>(type: "int", nullable: false),
                    DespachadorNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CombustibleRequerido = table.Column<double>(type: "float", nullable: false),
                    CargaPago = table.Column<double>(type: "float", nullable: false),
                    Ruta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClimaReporte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Despachos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Despachos_Vuelos_VueloId",
                        column: x => x.VueloId,
                        principalTable: "Vuelos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Aeronaves",
                columns: new[] { "Id", "CapacidadPasajeros", "Estado", "Matricula", "Modelo" },
                values: new object[,]
                {
                    { 1, 170, "Activa", "LV-CUX", "Boeing 737-800" },
                    { 2, 186, "Activa", "LV-FCR", "Boeing 737-800 MAX" },
                    { 3, 96, "Activa", "LV-GKO", "Embraer 190" },
                    { 4, 272, "Mantenimiento", "LV-FVL", "Airbus A330-200" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Contrasena", "NombreCompleto", "NombreUsuario", "Rol" },
                values: new object[,]
                {
                    { 1, "admin123", "Administrador del Sistema", "admin", "Despachador" },
                    { 2, "despacho123", "Juan Pérez (Despachador)", "despacho", "Despachador" },
                    { 3, "piloto123", "Martín Gómez (Comandante)", "piloto", "Piloto" }
                });

            migrationBuilder.InsertData(
                table: "Vuelos",
                columns: new[] { "Id", "AeronaveId", "Destino", "Estado", "FechaHoraLlegada", "FechaHoraSalida", "NumeroVuelo", "Origen" },
                values: new object[,]
                {
                    { 1, 4, "MAD (Madrid)", "Programado", new DateTime(2026, 6, 3, 14, 10, 0, 0, DateTimeKind.Local), new DateTime(2026, 6, 2, 23, 55, 0, 0, DateTimeKind.Local), "AR1300", "EZE (Buenos Aires)" },
                    { 2, 1, "COR (Córdoba)", "Programado", new DateTime(2026, 6, 2, 9, 50, 0, 0, DateTimeKind.Local), new DateTime(2026, 6, 2, 8, 30, 0, 0, DateTimeKind.Local), "AR1420", "AEP (Buenos Aires)" },
                    { 3, 2, "FTE (El Calafate)", "Programado", new DateTime(2026, 6, 2, 13, 35, 0, 0, DateTimeKind.Local), new DateTime(2026, 6, 2, 10, 15, 0, 0, DateTimeKind.Local), "AR1844", "AEP (Buenos Aires)" }
                });

            migrationBuilder.InsertData(
                table: "Despachos",
                columns: new[] { "Id", "CargaPago", "ClimaReporte", "CombustibleRequerido", "DespachadorNombre", "FechaCreacion", "Observaciones", "Ruta", "VueloId" },
                values: new object[] { 1, 12500.0, "SABE 010600Z 12008KT 9999 FEW030 18/14 Q1015; SACO 010600Z 09005KT 9999 SKC 15/10 Q1016", 4500.0, "Juan Pérez (Despachador)", new DateTime(2026, 6, 2, 6, 0, 0, 0, DateTimeKind.Local), "Vuelo despachado con desvío estándar por clima favorable. Combustible incluye reserva regulada de 45 minutos.", "AEP UT312 COR", 2 });

            migrationBuilder.CreateIndex(
                name: "IX_Despachos_VueloId",
                table: "Despachos",
                column: "VueloId");

            migrationBuilder.CreateIndex(
                name: "IX_Vuelos_AeronaveId",
                table: "Vuelos",
                column: "AeronaveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Despachos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Vuelos");

            migrationBuilder.DropTable(
                name: "Aeronaves");
        }
    }
}
