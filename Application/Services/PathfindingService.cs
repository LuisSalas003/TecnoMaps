using TechoMaps.Domain.Models;

namespace TechoMaps.Application.Services;

public class PathfindingService
{
    private readonly MapBuilderService _map;
    public PathfindingService(MapBuilderService map) { _map = map; }

    public List<Intersection> FindRoute(Coordinate startCoord, Coordinate endCoord, List<StreetSegment>? avoidStreets = null)
    {
        if (!_map.Intersections.ContainsKey(startCoord) || !_map.Intersections.ContainsKey(endCoord)) return new();

        var start = _map.Intersections[startCoord];
        var end = _map.Intersections[endCoord];

        var openSet = new List<Intersection> { start };
        var cameFrom = new Dictionary<Intersection, Intersection>();
        var gScore = new Dictionary<Intersection, int> { [start] = 0 };

        while (openSet.Any())
        {
            var current = openSet.OrderBy(n => gScore.GetValueOrDefault(n, 9999)).First();
            if (current == end) return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            EvaluateNeighbors(current, openSet, cameFrom, gScore, avoidStreets);
        }
        return new();
    }

    private void EvaluateNeighbors(Intersection current, List<Intersection> openSet, Dictionary<Intersection, Intersection> cameFrom, Dictionary<Intersection, int> gScore, List<StreetSegment>? avoidStreets)
    {
        var neighbors = _map.Streets.Where(s => s.Start == current).ToList();

        foreach (var street in neighbors)
        {
            if (street.IsBlocked) continue;

            int penaltyExtra = (avoidStreets != null && avoidStreets.Contains(street)) ? 10 : 0;
            var neighbor = street.End;
            int tentativeGScore = gScore[current] + street.TotalTime + penaltyExtra;

            if (tentativeGScore < gScore.GetValueOrDefault(neighbor, 9999))
            {
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
            }
        }
    }

    // Hecho estático (S2325)
    private static List<Intersection> ReconstructPath(Dictionary<Intersection, Intersection> cameFrom, Intersection current)
    {
        var path = new List<Intersection> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    public List<StreetSegment> GetSegmentsFromPath(List<Intersection> path)
    {
        var segments = new List<StreetSegment>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            var s = _map.Streets.FirstOrDefault(st => st.Start == path[i] && st.End == path[i+1]);
            if (s != null) segments.Add(s);
        }
        return segments;
    }

    public StreetSegment? GetSegment(Intersection start, Intersection end)
    {
        return _map.Streets.FirstOrDefault(s => s.Start == start && s.End == end);
    }
}