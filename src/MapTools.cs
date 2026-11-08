using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Puchatek.FlotsamPlus
{
    internal static class MapTools
    {
        private static FieldInfo _instances, _colors, _mesh;

        internal static void Initialize()
        {
            _instances = AccessTools.Field(typeof(WorldMapFogOfWar), "_instances");
            _colors = AccessTools.Field(typeof(WorldMapFogOfWar), "_colors");
            _mesh = AccessTools.Field(typeof(WorldMapFogOfWar), "_mesh");
            if (_instances == null || !_instances.IsStatic ||
                _instances.FieldType != typeof(List<WorldMapFogOfWar>) ||
                _colors == null || _colors.FieldType != typeof(Color[]) ||
                _mesh == null || _mesh.FieldType != typeof(Mesh))
                throw new InvalidOperationException("Nieznana wersja mapy i mgły wojny.");
        }

        internal static string Reveal(World world)
        {
            if (world == null || Community.PlayerCommunity == null)
                throw new InvalidOperationException("Najpierw wczytaj zapis gry.");
            var tiles = new List<WorldTile>(world.Tiles);
            int landmarks = 0, pointsOfInterest = 0, fogs = 0;
            foreach (WorldTile tile in tiles)
            {
                foreach (LandmarkSpawner landmark in new List<LandmarkSpawner>(tile.Landmarks))
                {
                    // Disabled narrative spawners belong to the quest system.
                    // Reveal existing locations without activating those spawners.
                    if (landmark == null || !landmark.Enabled ||
                        landmark.ScoutingState >= ScoutingState.Scouted) continue;
                    landmark.SetScoutingState(ScoutingState.Scouted);
                    ++landmarks;
                }
                foreach (PointOfInterestSpawner point in
                    new List<PointOfInterestSpawner>(tile.PointsOfInterest))
                {
                    if (point == null || !point.HasPosition ||
                        point.ScoutingState >= ScoutingState.Scouted) continue;
                    point.SetScoutingState(ScoutingState.Scouted);
                    ++pointsOfInterest;
                }
            }
            var instances = _instances.GetValue(null) as List<WorldMapFogOfWar>;
            if (instances != null)
                foreach (WorldMapFogOfWar fog in new List<WorldMapFogOfWar>(instances))
                {
                    if (fog == null || fog.WorldTile == null || !tiles.Contains(fog.WorldTile)) continue;
                    Color[] colors = _colors.GetValue(fog) as Color[];
                    Mesh mesh = _mesh.GetValue(fog) as Mesh;
                    // A newly added map tile may still be initializing. The
                    // next automatic pass handles it after its mesh is ready.
                    if (colors == null || mesh == null) continue;
                    bool changed = false;
                    for (int i = 0; i < colors.Length; ++i)
                        if (colors[i].a != 0f) { colors[i].a = 0f; changed = true; }
                    if (!changed) continue;
                    mesh.colors = colors;
                    // ReturnAlphas() reads _colors, so the normal game save
                    // persists this discovery as well as the visible mesh.
                    ++fogs;
                }
            return "Odkryto nowe miejsca: " + (landmarks + pointsOfInterest) +
                ". Odsłonięte fragmenty mapy: " + fogs + ". Dostępne części świata: " + tiles.Count + ".";
        }
    }
}
