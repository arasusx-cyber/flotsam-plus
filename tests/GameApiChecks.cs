using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class GameApiChecks
{
    private static int _checks;
    private static AssemblyDefinition _game;
    private static TypeDefinition Type(string name) =>
        _game.MainModule.GetType(name) ?? throw new Exception("Missing game type: " + name);

    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception("FAIL: " + name);
        ++_checks;
        Console.WriteLine("PASS: " + name);
    }

    private static MethodDefinition Method(string type, string name, string returns, params string[] args)
    {
        MethodDefinition method = Type(type).Methods.SingleOrDefault(m =>
            m.Name == name && m.ReturnType.FullName == returns &&
            m.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(args));
        Check(method != null, type + "." + name + " has the required signature");
        return method;
    }

    private static int Reads(MethodDefinition method, string owner, string field)
    {
        return method.Body.Instructions.Count(i => (i.OpCode == OpCodes.Ldfld || i.OpCode == OpCodes.Ldsfld) &&
            i.Operand is FieldReference f && f.DeclaringType.FullName == owner && f.Name == field);
    }

    private static bool Calls(MethodDefinition method, string owner, string name)
    {
        return method.Body.Instructions.Any(i => i.Operand is MethodReference call &&
            call.DeclaringType.FullName == owner && call.Name == name);
    }

    private static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Usage: GameApiChecks GAME_ASSEMBLY BUILT_PLUGIN");
            return 2;
        }
        try
        {
            _game = AssemblyDefinition.ReadAssembly(args[0]);
            Method("WorldMap", "get_MovementSpeed", "System.Single");
            Method("WorldMap", "get_RotationSpeed", "System.Single");
            MethodDefinition coroutine = Method("WorldMap", "MoveToCoroutine",
                "System.Collections.IEnumerator", "MapPath");
            TypeDefinition iterator = Type("WorldMap").NestedTypes.Single(t =>
                t.Name.StartsWith("<MoveToCoroutine>", StringComparison.Ordinal));
            MethodDefinition moveNext = iterator.Methods.Single(m => m.Name == "MoveNext");
            Check(Reads(moveNext, "WorldMap", "_movementSpeed") == 2 &&
                Reads(moveNext, "WorldMap", "_rotationSpeed") == 1,
                "Path movement has exactly the three field reads replaced by the transpiler");

            Method("Engine", "ConsumeEnergy", "System.Boolean", "System.Single");
            Method("Engine", "ReturnEnergyCost", "System.Single", "System.Single");
            Method("Engine", "ReturnEnergyRange", "System.Single", "System.Single");
            Method("Engine", "ReturnMoveableDistance", "System.Single", "System.Single");
            Method("WorldMapTownheartPhycis2D", "FixedUpdate", "System.Void");
            Method("WorldMapTownheartPhycis2D", "AddForceAndTorque", "System.Void",
                "System.Single", "System.Single");
            foreach (var field in new[] {
                ("_rigidbody", "UnityEngine.Rigidbody2D"),
                ("_engine", "Engine"),
                ("_mode", "WorldMapTownheartPhycis2D/Modes") })
                Check(Type("WorldMapTownheartPhycis2D").Fields.Any(f =>
                    f.Name == field.Item1 && f.FieldType.FullName == field.Item2),
                    "Physics injection field " + field.Item1 + " matches");
            Check(Type("WorldMapTownheartPhycis2D").NestedTypes.Single(t => t.Name == "Modes")
                .Fields.Any(f => f.Name == "Force"), "Physical acceleration mode exists");
            Method("WorldMapTownheartPhycis2D", "get_ProcessedInput", "System.Boolean");

            var building = new List<MethodDefinition>();
            foreach (string name in new[] { "StartBuilding", "Upgrade", "TryToStartUpgrade", "StartUpgrade" })
                building.Add(Method("Buildable", name, "System.Void"));
            building.Add(Method("ConstructionHandler", "StartBuilding", "System.Void", "System.Boolean"));
            building.Add(Method("BoatCursorProperties", "UpdateCursor", "System.Void", "CursorManager"));
            building.Add(Type("LineConstructionCursorProperties").Methods.Single(m =>
                m.Name == "PlaceLine" && m.Parameters.Count == 4));
            building.Add(Method("ResourceManager", "AreCommunityResourcesAvailable", "System.Boolean",
                "CountedItemProperty[]"));
            foreach (MethodDefinition method in building)
                Check(Reads(method, "BuildingDevTools", "InstantBuild") > 0,
                    "Native instant-build branch exists in " + method.DeclaringType.Name + "." + method.Name);
            var owners = new HashSet<string> {
                "Buildable", "ConstructionHandler", "BoatCursorProperties",
                "LineConstructionCursorProperties", "ResourceManager"
            };
            int flagReaders = _game.MainModule.Types.Where(t => owners.Contains(t.FullName))
                .SelectMany(t => t.Methods).Count(m =>
                    m.HasBody && Reads(m, "BuildingDevTools", "InstantBuild") > 0);
            Check(flagReaders == building.Count, "Every native instant-build flag reader is covered");
            Check(Type("ConstructionHandler").Fields.Any(f => f.Name == "_owner" &&
                f.FieldType.FullName == "System.WeakReference" + ((char)96) + "1<IConstructible>"),
                "Construction owner field matches the player-scope check");
            Method("IConstructible", "get_Community", "Community");

            MethodDefinition research = Method("ResearchStation", "Research", "System.Boolean", "Agent");
            Check(research.Body.Instructions.Count(i => i.Operand is MethodReference call &&
                call.DeclaringType.FullName == "UnityEngine.Time" && call.Name == "get_deltaTime") == 1,
                "Research has exactly one delta-time read; native progress and completion stay intact");
            Method("ResearchStation", "get_Buildable", "Buildable");
            Method("PajamaLlama.Fltsm.Rest", "OnNighttimeStarted", "System.Void", "GameEvent");
            Method("Agent", "get_IsAlive", "System.Boolean");
            Method("Buildable", "get_Community", "Community");
            Method("Engine", "get_Buildable", "Buildable");

            Method("Navigator", "ReturnSpeed", "System.Single", "Navigator/TerrainType");
            Check(Type("Navigator").Fields.Any(f => f.Name == "_agent" &&
                f.FieldType.FullName == "Agent"), "Resident speed patch injects the native Agent owner");
            MethodDefinition defaultSpeed = Method("Navigator", "ReturnSpeed", "System.Single");
            Check(Calls(defaultSpeed, "Navigator", "ReturnSpeed"),
                "Parameterless movement delegates to the patched terrain getter");
            MethodDefinition follow = Method("PathfinderNavigator", "FollowPathFixedDeltaTime",
                "System.Void", "GraphBase", "System.Single");
            Check(Calls(follow, "Navigator", "ReturnSpeed"),
                "Construction and water paths consume the native speed getter");
            Method("UnityNavMeshNavigator", "Update", "System.Void");
            Method("UnityNavMeshNavigator", "get_Navigator", "Navigator");
            Method("UnityNavMeshNavigator", "get_UnityNavMeshAgent", "UnityEngine.AI.NavMeshAgent");
            Check(Calls(Type("Navigator").Methods.Single(m => m.Name == "UpdateTerrain"),
                "UnityEngine.AI.NavMeshAgent", "set_speed"),
                "Island navigation caches speed, requiring the live refresh patch");

            Method("DrifterAttributes", "get_SpendablePoints", "System.Int32");
            Check(Type("DrifterAttributes").Fields.Any(f =>
                f.Name == "<SpendablePoints>k__BackingField" && f.FieldType.FullName == "System.Int32" &&
                !f.IsStatic), "Skill grants target the per-resident unspent counter");
            Method("DrifterAttributes", "get_AvailableSpendingPointsUpdatedEvent",
                "UnityEngine.Events.UnityEvent");
            TypeDefinition persisted = Type("DrifterAttributes").NestedTypes.Single(t => t.Name == "PersistentData");
            Check(persisted.Fields.Any(f => f.Name == "SpendablePoints" &&
                f.CustomAttributes.Any(a => a.AttributeType.Name == "DataMemberAttribute")),
                "The ordinary save format includes unspent skill points");
            Check(Calls(persisted.Methods.Single(m => m.IsConstructor),
                "DrifterAttributes", "get_SpendablePoints") &&
                Calls(persisted.Methods.Single(m => m.Name == "Restore"),
                "DrifterAttributes", "set_SpendablePoints"),
                "Native saving and restoring preserve the unspent counter directly");
            Method("LandmarkSpawner", "SetScoutingState", "System.Void", "ScoutingState");
            Method("PointOfInterestSpawner", "SetScoutingState", "System.Void", "ScoutingState");
            foreach (var field in new[] {
                ("_instances", "System.Collections.Generic.List`1<WorldMapFogOfWar>"),
                ("_colors", "UnityEngine.Color[]"), ("_mesh", "UnityEngine.Mesh") })
                Check(Type("WorldMapFogOfWar").Fields.Any(f =>
                    f.Name == field.Item1 && f.FieldType.FullName == field.Item2),
                    "Fog reveal field " + field.Item1 + " matches the native map");
            MethodDefinition alphas = Method("WorldMapFogOfWar", "ReturnAlphas", "System.Byte[]");
            Check(Reads(alphas, "WorldMapFogOfWar", "_colors") > 0,
                "The native fog serializer reads the same colors that reveal updates");
            Method("WorldMapFogOfWar", "get_WorldTile", "WorldTile");

            using (AssemblyDefinition plugin = AssemblyDefinition.ReadAssembly(args[1]))
            {
                Check(plugin.Name.Version.ToString() == "1.2.0.0", "Built plugin has release version 1.2.0");
                string[] expected = {
                    "ShipSpeedPatch", "ShipTurningPatch", "PathMovementPatch", "TravelEnergyPatch",
                    "TravelCostPatch", "TravelRangePatch", "TravelCooldownPatch", "InstantBuildingPatch",
                    "ResearchSpeedPatch", "RestNightPatch", "ShipPhysicsPatch", "ShipForcePatch",
                    "ResidentSpeedPatch", "ResidentNavMeshSpeedPatch"
                };
                Check(expected.All(n => plugin.MainModule.Types.Any(t =>
                    t.Name == n && t.CustomAttributes.Any(a => a.AttributeType.Name == "HarmonyPatch"))),
                    "All fourteen gameplay Harmony patch classes are included in the binary");
                MethodDefinition grant = plugin.MainModule.Types.Single(t => t.Name == "ResidentTools")
                    .Methods.Single(m => m.Name == "AddPoints");
                Check(Calls(grant, "DrifterAttributes", "get_AvailableSpendingPointsUpdatedEvent") &&
                    !Calls(grant, "DrifterAttributes", "AddLevel") &&
                    !Calls(grant, "DrifterAttributes", "AddTotalExperience"),
                    "The built skill action refreshes the native UI without granting XP or levels");
                MethodDefinition reveal = plugin.MainModule.Types.Single(t => t.Name == "MapTools")
                    .Methods.Single(m => m.Name == "Reveal");
                Check(!reveal.Body.Instructions.Any(i => i.Operand is MethodReference call &&
                    (call.Name == "Scout" || call.Name == "StartQuest" || call.Name == "AddNextTile")),
                    "Map reveal does not force scouting quests or generate future world tiles");
                Check(!plugin.MainModule.AssemblyReferences.Any(a => a.Name == "Mono.Cecil"),
                    "Game API checker dependency is not included in the plugin");
            }
            Console.WriteLine("Passed " + _checks + " game API checks; gameplay not executed.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error.ToString());
            return 1;
        }
    }
}

