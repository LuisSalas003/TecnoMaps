using TechoMaps.Domain.Models;

namespace TechoMaps.Application.Services;

public class MapBuilderService
{
    public Dictionary<Coordinate, Intersection> Intersections { get; } = new();
    public List<StreetSegment> Streets { get; } = new();
    public Dictionary<Coordinate, IncidentType> ActiveIncidents { get; } = new();

    public void BuildCity()
    {
        CreateIntersections();
        ConnectStreets();
    }

    private void CreateIntersections()
    {
        for (int x = 0; x <= 10; x++)
            for (int y = 0; y <= 10; y++)
                Intersections[new Coordinate(x, y)] = new Intersection($"({x},{y})", new Coordinate(x, y));
    }

    private void ConnectStreets()
    {
        for (int x = 0; x <= 10; x++)
        {
            for (int y = 0; y <= 10; y++)
            {
                var current = Intersections[new Coordinate(x, y)];
                ConnectHorizontal(x, y, current);
                ConnectVertical(x, y, current);
            }
        }
    }

    private void ConnectHorizontal(int x, int y, Intersection current)
    {
        if (x < 10 && y % 2 == 0) Streets.Add(new StreetSegment(current, Intersections[new Coordinate(x + 1, y)]));
        else if (x > 0 && y % 2 != 0) Streets.Add(new StreetSegment(current, Intersections[new Coordinate(x - 1, y)]));
    }

    private void ConnectVertical(int x, int y, Intersection current)
    {
        if (y < 10 && x % 2 == 0) Streets.Add(new StreetSegment(current, Intersections[new Coordinate(x, y + 1)]));
        else if (y > 0 && x % 2 != 0) Streets.Add(new StreetSegment(current, Intersections[new Coordinate(x, y - 1)]));
    }

    // Hecho estático (S2325)
    private static bool IsIn3x3(Coordinate coord, Coordinate center)
    {
        return Math.Abs(coord.X - center.X) <= 1 && Math.Abs(coord.Y - center.Y) <= 1;
    }

    public void AddIncident(Coordinate center, IncidentType type)
    {
        ActiveIncidents[center] = type;
        
        // Bucle simplificado con LINQ Where (S3267)
        var affectedStreets = Streets.Where(s => IsIn3x3(s.Start.Location, center) || IsIn3x3(s.End.Location, center));
        foreach (var street in affectedStreets)
        {
            street.ApplyIncident(type); 
        }
    }

    private bool IsNodeInAffectedArea(Coordinate coord)
    {
        // Bucle simplificado con LINQ Any (S3267)
        return ActiveIncidents.Keys.Any(incidentCenter => IsIn3x3(coord, incidentCenter));
    }

    // Hecho estático (S2325)
    private static void PrintLegend(string symbol, string text, ConsoleColor color)
    {
        Console.Write("[");
        Console.ForegroundColor = color;
        Console.Write(symbol);
        Console.ResetColor();
        Console.Write($" {text}]");
    }

    public void DrawMap(Coordinate? startCoord = null, Coordinate? endCoord = null, List<Intersection>? path1 = null, List<Intersection>? path2 = null)
    {
        Console.WriteLine("\n  0 1 2 3 4 5 6 7 8 9 10 (X)");
        for (int y = 0; y <= 10; y++)
        {
            Console.Write(y.ToString().PadLeft(2) + " ");
            for (int x = 0; x <= 10; x++)
            {
                DrawNode(new Coordinate(x, y), startCoord, endCoord, path1, path2);
            }
            Console.WriteLine();
        }
        
        PrintFullLegend();
    }

    private void DrawNode(Coordinate coord, Coordinate? startCoord, Coordinate? endCoord, List<Intersection>? path1, List<Intersection>? path2)
    {
        var intersection = Intersections[coord];
        bool isAffected = IsNodeInAffectedArea(coord);
        
        string symbol = GetSymbolForNode(coord, startCoord, endCoord, isAffected);
        ConsoleColor color = GetColorForNode(coord, startCoord, endCoord, intersection, isAffected, path1, path2);

        Console.ForegroundColor = color;
        Console.Write(symbol);
        Console.ResetColor();
    }

    private string GetSymbolForNode(Coordinate coord, Coordinate? startCoord, Coordinate? endCoord, bool isAffected)
    {
        if (startCoord != null && coord == startCoord) return "I ";
        if (endCoord != null && coord == endCoord) return "D ";
        if (isAffected && !ActiveIncidents.ContainsKey(coord)) return "* ";
        return "O ";
    }

    private ConsoleColor GetColorForNode(Coordinate coord, Coordinate? startCoord, Coordinate? endCoord, Intersection intersection, bool isAffected, List<Intersection>? path1, List<Intersection>? path2)
    {
        if (coord == startCoord || coord == endCoord) return ConsoleColor.White;
        
        if (ActiveIncidents.TryGetValue(coord, out IncidentType type))
        {
            return type switch
            {
                IncidentType.BloqueoCalle => ConsoleColor.Red,
                IncidentType.TraficoIntenso => ConsoleColor.Magenta,
                _ => ConsoleColor.Cyan
            };
        }

        if (path1 != null && path1.Contains(intersection)) return ConsoleColor.Green;
        if (path2 != null && path2.Contains(intersection)) return ConsoleColor.Blue;
        if (isAffected) return ConsoleColor.DarkYellow;
        
        return ConsoleColor.DarkGray; 
    }

   // Hecho estático (S2325)
    private static void PrintFullLegend()
    {
        // Se cambió la frase para arreglar el falso positivo del T-O-D-O (S1135)
        // --- LEYENDA VISUAL COMPLETA ---
        Console.WriteLine();
        Console.Write("Calles:   ");
        PrintLegend("O", "Gris", ConsoleColor.DarkGray); Console.Write(" Normal | ");
        PrintLegend("O", "Verde", ConsoleColor.Green); Console.Write(" Ruta 1 | ");
        PrintLegend("O", "Azul", ConsoleColor.Blue); Console.Write(" Ruta 2 | ");
        PrintLegend("*", "Naranja", ConsoleColor.DarkYellow); Console.WriteLine(" Área 3x3");
        
        Console.Write("Viaje:    ");
        PrintLegend("I", "Blanco", ConsoleColor.White); Console.Write(" Inicio | ");
        PrintLegend("D", "Blanco", ConsoleColor.White); Console.WriteLine(" Destino");

        Console.Write("Sucesos:  ");
        PrintLegend("O", "Rojo", ConsoleColor.Red); Console.Write(" Bloqueo | ");
        PrintLegend("O", "Magenta", ConsoleColor.Magenta); Console.Write(" Tráfico | ");
        PrintLegend("O", "Cyan", ConsoleColor.Cyan); Console.WriteLine(" Radar\n");
    }
}