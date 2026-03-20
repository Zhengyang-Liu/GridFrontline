using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GridFrontline;

/// <summary>
/// Tracks all active units on the battlefield.
/// Provides spatial queries for target finding.
/// </summary>
public partial class UnitManager : Node
{
    private readonly List<Unit> _units = new();

    public void Register(Unit unit)
    {
        if (!_units.Contains(unit))
            _units.Add(unit);
    }

    public void Unregister(Unit unit)
    {
        _units.Remove(unit);
    }

    /// <summary>
    /// Find the nearest enemy unit within search range.
    /// </summary>
    public Unit GetNearestEnemy(Vector2 position, Team myTeam, float range)
    {
        Unit nearest = null;
        float nearestDist = range;

        foreach (var unit in _units)
        {
            if (unit.UnitTeam == myTeam) continue;
            if (unit.State == Unit.UnitState.Dead || unit.State == Unit.UnitState.InRally) continue;

            float dist = position.DistanceTo(unit.GlobalPosition);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = unit;
            }
        }
        return nearest;
    }

    /// <summary>
    /// Get all enemy units within range (for AOE skills).
    /// </summary>
    public List<Unit> GetEnemiesInRange(Vector2 position, Team myTeam, float range)
    {
        var result = new List<Unit>();
        foreach (var unit in _units)
        {
            if (unit.UnitTeam == myTeam) continue;
            if (unit.State == Unit.UnitState.Dead || unit.State == Unit.UnitState.InRally) continue;

            if (position.DistanceTo(unit.GlobalPosition) <= range)
                result.Add(unit);
        }
        return result;
    }

    /// <summary>
    /// Get all alive units for a given team.
    /// </summary>
    public List<Unit> GetAliveUnits(Team team)
    {
        return _units.Where(u => u.UnitTeam == team
            && u.State != Unit.UnitState.Dead
            && u.State != Unit.UnitState.InRally).ToList();
    }

    /// <summary>
    /// Clean up dead units from the tracking list.
    /// Called periodically by GameManager.
    /// </summary>
    public void CleanupDead()
    {
        _units.RemoveAll(u => u.State == Unit.UnitState.Dead);
    }
}
