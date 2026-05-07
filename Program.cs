using TechoMaps.Domain.Models;
using TechoMaps.Application.Services;
using TechoMaps.Presentation; // <-- Agregamos esta línea para usar el Helper

Console.Clear();
Console.WriteLine("=== TECHO MAPS (Sistema Experto Monótono) ===");

var map = new MapBuilderService();
map.BuildCity();

ConsoleHelper.ConfigurarIncidencias(map);
ConsoleHelper.ConfigurarPuntosViaje(out Coordinate start, out Coordinate end);

var pf = new PathfindingService(map);
var route1 = pf.FindRoute(start, end);
var route2 = pf.FindRoute(start, end, pf.GetSegmentsFromPath(route1));

Console.Clear();
Console.WriteLine("=== RESULTADOS DE TECHO MAPS ===");
map.DrawMap(start, end, route1, route2);

ConsoleHelper.ImprimirDesgloseRuta("Ruta 1 (Óptima - Verde)", route1, pf, ConsoleColor.Green);
ConsoleHelper.ImprimirDesgloseRuta("Ruta 2 (Alternativa - Azul)", route2, pf, ConsoleColor.Blue);

Console.WriteLine("\nPresiona cualquier tecla para finalizar...");
Console.ReadKey();


// --- CLASE AUXILIAR DENTRO DE SU PROPIO NAMESPACE ---
namespace TechoMaps.Presentation
{
    public static class ConsoleHelper
    {
        public static void ConfigurarIncidencias(MapBuilderService mapService)
        {
            int numIncidencias = LeerEntero("¿Cuántas incidencias hay actualmente en la ciudad? (0-10): ", 0, 10);
            for (int i = 0; i < numIncidencias; i++)
            {
                Console.WriteLine($"\n--- Configurando Incidencia {i + 1} ---");
                int ix = LeerEntero("Coordenada X (0-10): ", 0, 10);
                int iy = LeerEntero("Coordenada Y (0-10): ", 0, 10);
                int type = LeerEntero("Tipo (1: Bloqueo, 2: Tráfico, 3: Radar): ", 1, 3);
                
                IncidentType incident = type switch
                {
                    1 => IncidentType.BloqueoCalle,
                    2 => IncidentType.TraficoIntenso,
                    _ => IncidentType.RadarPolicia
                };
                
                mapService.AddIncident(new Coordinate(ix, iy), incident);
            }
        }

        public static void ConfigurarPuntosViaje(out Coordinate startCoord, out Coordinate endCoord)
        {
            Console.WriteLine("\n--- Configuración de Puntos de Viaje ---");
            int ox = LeerEntero("Origen X (0-10): ", 0, 10);
            int oy = LeerEntero("Origen Y (0-10): ", 0, 10);
            int dx = LeerEntero("Destino X (0-10): ", 0, 10);
            int dy = LeerEntero("Destino Y (0-10): ", 0, 10);

            startCoord = new Coordinate(ox, oy);
            endCoord = new Coordinate(dx, dy);
        }

        private static int LeerEntero(string mensaje, int min, int max)
        {
            while (true)
            {
                Console.Write(mensaje);
                if (int.TryParse(Console.ReadLine(), out int valor) && valor >= min && valor <= max)
                    return valor;
                
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Por favor ingresa un número válido entre {min} y {max}.");
                Console.ResetColor();
            }
        }

        public static void ImprimirDesgloseRuta(string nombre, List<Intersection> ruta, PathfindingService pathService, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"\n=== DESGLOSE DE {nombre.ToUpper()} ===");
            Console.ResetColor();

            if (ruta.Count == 0)
            {
                Console.WriteLine("No se encontró una ruta viable. Es posible que el destino esté completamente bloqueado.");
                return;
            }

            int tiempoTotal = 0;
            for (int i = 0; i < ruta.Count - 1; i++)
            {
                var tramo = pathService.GetSegment(ruta[i], ruta[i + 1]);
                if (tramo != null)
                {
                    Console.WriteLine($"Paso {i + 1}: Dirígete de {ruta[i].Name} a {ruta[i + 1].Name} -> Toma {tramo.TotalTime} min");
                    tiempoTotal += tramo.TotalTime;
                }
            }
            
            Console.ForegroundColor = color;
            Console.WriteLine($">> TIEMPO TOTAL ESTIMADO: {tiempoTotal} minutos <<");
            Console.ResetColor();
        }
    }
}