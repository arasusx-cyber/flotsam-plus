using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Puchatek.FlotsamPlus
{
    internal static class ResidentTools
    {
        private static FieldInfo _points;

        internal static void InitializePoints()
        {
            _points = AccessTools.Field(typeof(DrifterAttributes), "<SpendablePoints>k__BackingField");
            if (_points == null || _points.FieldType != typeof(int) || _points.IsStatic)
                throw new InvalidOperationException("Nieznany licznik punktów umiejętności.");
        }

        internal static bool IsResident(Agent agent, Community community)
        {
            return agent != null && agent.Initialized && agent.IsAlive &&
                Plugin.IsPlayer(community) && ReferenceEquals(agent.Community, community);
        }

        internal static int AddPoints(Agent agent, Community community, int amount)
        {
            if (!IsResident(agent, community) || !community.Agents.Contains(agent) ||
                agent.Attributes == null || _points == null)
                throw new InvalidOperationException("Wybierz żywego mieszkańca bieżącej osady.");
            DrifterAttributes attributes = agent.Attributes;
            if (!PointGrant.TryAdd(attributes.SpendablePoints, amount, out int next))
                throw new ArgumentOutOfRangeException(nameof(amount), "Wpisz od 1 do 100 punktów.");
            // The normal save serializer stores this exact field. Grant only
            // unspent points: XP, level, research points and skill caps stay native.
            _points.SetValue(attributes, next);
            attributes.AvailableSpendingPointsUpdatedEvent?.Invoke();
            return next;
        }

        internal static void RefreshNavMesh(UnityNavMeshNavigator navigation)
        {
            if (navigation == null || navigation.Navigator == null ||
                navigation.UnityNavMeshAgent == null) return;
            Agent agent = navigation.Navigator.GetComponent<Agent>();
            if (!IsResident(agent, Community.PlayerCommunity)) return;
            // Unity's navigation caches its speed when changing terrain. Read
            // the same native getter each time, rather than multiplying a cache.
            navigation.UnityNavMeshAgent.speed =
                navigation.Navigator.ReturnSpeed(Navigator.TerrainType.UnityNavMesh);
        }

        internal static void RefreshNavMeshes(Community community)
        {
            if (!Plugin.IsPlayer(community)) return;
            foreach (Agent agent in new List<Agent>(community.Agents))
            {
                if (!IsResident(agent, community)) continue;
                Navigator navigation = agent.ReturnNavigator();
                if (navigation != null)
                    RefreshNavMesh(navigation.GetComponent<UnityNavMeshNavigator>());
            }
        }
    }

    [HarmonyPatch(typeof(Navigator), nameof(Navigator.ReturnSpeed),
        new[] { typeof(Navigator.TerrainType) })]
    internal static class ResidentSpeedPatch
    {
        private static void Postfix(Agent ____agent, ref float __result)
        {
            Plugin plugin = Plugin.Instance;
            if (plugin != null && plugin.ResidentsReady &&
                ResidentTools.IsResident(____agent, Community.PlayerCommunity))
                __result = Tuning.Scale(__result, plugin.ResidentSpeed.Value);
        }
    }

    [HarmonyPatch(typeof(UnityNavMeshNavigator), "Update")]
    internal static class ResidentNavMeshSpeedPatch
    {
        private static void Prefix(UnityNavMeshNavigator __instance)
        {
            Plugin plugin = Plugin.Instance;
            if (plugin != null && plugin.ResidentsReady)
                ResidentTools.RefreshNavMesh(__instance);
        }
    }
}
