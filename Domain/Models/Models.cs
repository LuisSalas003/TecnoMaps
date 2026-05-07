namespace TechoMaps.Domain.Models;

public record Coordinate(int X, int Y);

public enum IncidentType { BloqueoCalle, TraficoIntenso, RadarPolicia }

public class Intersection
{
    public string Name { get; }
    public Coordinate Location { get; }
    public Intersection(string name, Coordinate location) { Name = name; Location = location; }
}

public class StreetSegment
{
    public Intersection Start { get; }
    public Intersection End { get; }
    public int BaseTime { get; } = 1;
    public int PenaltyTime { get; private set; } = 0;
    public bool IsBlocked { get; private set; } = false;

    // Lógica Monótona: El costo es el tiempo base + penalizaciones. Si está bloqueado, es "infinito" (9999 para evitar desbordes).
    public int TotalTime => IsBlocked ? 9999 : BaseTime + PenaltyTime;

    public StreetSegment(Intersection start, Intersection end) { Start = start; End = end; }

    public void ApplyIncident(IncidentType type)
    {
        if (type == IncidentType.BloqueoCalle) IsBlocked = true;
        else if (PenaltyTime == 0) PenaltyTime = 2; // Suma 2 para que el total sea 3 min.
    }
}