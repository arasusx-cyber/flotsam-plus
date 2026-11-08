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

            MethodDefinition capacity = Method("Engine", "ReturnTownTugCapacity", "System.Single");
            MethodDefinition engineLate = Method("Engine", "LateUpdate", "System.Void");
            Check(Calls(engineLate, "Engine", "ReturnTownTugCapacity") &&
                Calls(engineLate, "Engine", "set_TownAvailableTugCapacity") &&
                Calls(engineLate, "Engine", "set_TownUsedTugCapacityPercentage"),
                "The native frame update refreshes available capacity and percentage from the patched result");
            Check(capacity.HasBody && Reads(capacity, "Engine", "_tugCapacity") > 0 &&
                Reads(capacity, "Engine", "_townTugCapacity") == 0,
                "Capacity is recomputed from native engine capacity rather than a scaled cache");
            MethodDefinition canTug = Method("Engine", "CanTug", "System.Boolean", "PlaceableProperties");
            Check(Type("Engine").Fields.Any(f => f.Name == "_townheartEngine" &&
                f.IsStatic && f.FieldType.FullName == "Engine"),
                "The unlimited patch can inject the current townheart engine for player scoping");
            Check(Reads(canTug, "Engine", "_townheartEngine") > 0 &&
                Calls(canTug, "PlaceableProperties", "GetWeightModeWeight") &&
                Calls(canTug, "Engine", "get_TownAvailableTugCapacity"),
                "Native CanTug checks the engine and compares placement weight with available capacity");
            string[] allowedTugCalls = { "UnityEngine.Object.op_Equality",
                "PlaceableProperties.GetWeightModeWeight", "Engine.get_TownAvailableTugCapacity" };
            Check(canTug.Body.Instructions.Where(i => i.Operand is MethodReference)
                .Select(i => (MethodReference)i.Operand)
                .All(m => allowedTugCalls.Contains(m.DeclaringType.FullName + "." + m.Name)),
                "The bypassed CanTug path contains no resource, unlock, connection or geometry rules");
            Method("Engine", "get_TownTugCapacity", "System.Single");
            Method("Engine", "get_TownWeight", "System.Single");
            MethodDefinition canPlace = Method("BuildableProperties", "ReturnCanBePlaced", "System.Boolean",
                "Community", "System.Boolean");
            Check(Calls(canPlace, "Engine", "CanTug") &&
                Calls(canPlace, "ResourceManager", "AreCommunityResourcesAvailable") &&
                Calls(canPlace, "Community", "IsThereAMooringPointFree") &&
                Calls(canPlace, "BuildableProperties", "ReturnBuildableRequirement"),
                "Weight permission is separate from native resource, mooring and buildable checks");
            Console.WriteLine("Capacity-related callers: " + string.Join(", ",
                _game.MainModule.Types.SelectMany(t => t.Methods)
                    .Where(m => m.HasBody && (Calls(m, "Engine", "CanTug") ||
                        Calls(m, "Engine", "get_TownAvailableTugCapacity")))
                    .Select(m => m.DeclaringType.FullName + "." + m.Name)));

            Method("FlotsamInputManager", "GetAxis", "System.Single", "System.Int32");
            Method("FlotsamInputManager", "GetAxisRaw", "System.Single", "System.Int32");
            Method("FlotsamInputManager", "GetCameraZoom", "System.Single");
            Method("FlotsamInputManager", "GetCameraRotation", "System.Single");

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
                Check(plugin.Name.Version.ToString() == "1.4.0.0", "Built plugin has release version 1.4.0");
                string[] expected = {
                    "ShipSpeedPatch", "ShipTurningPatch", "PathMovementPatch", "TravelEnergyPatch",
                    "TravelCostPatch", "TravelRangePatch", "TravelCooldownPatch", "InstantBuildingPatch",
                    "ResearchSpeedPatch", "RestNightPatch", "ShipPhysicsPatch", "ShipForcePatch",
                    "ResidentSpeedPatch", "ResidentNavMeshSpeedPatch",
                    "TownCapacityPatch", "UnlimitedTownWeightPatch"
                };
                Check(expected.All(n => plugin.MainModule.Types.Any(t =>
                    t.Name == n && t.CustomAttributes.Any(a => a.AttributeType.Name == "HarmonyPatch"))),
                    "All sixteen gameplay Harmony patch classes are included in the binary");
                foreach (string name in new[] { "TownCapacityPatch", "UnlimitedTownWeightPatch" })
                {
                    MethodDefinition callback = plugin.MainModule.Types.Single(t => t.Name == name)
                        .Methods.Single(m => m.Name == "Postfix" || m.Name == "Prefix");
                    Check(Calls(callback, "Puchatek.FlotsamPlus.Plugin", "IsPlayerEngine") &&
                        Reads(callback, "Puchatek.FlotsamPlus.Plugin", "CapacityReady") > 0,
                        name + " checks compatibility and player ownership in the built DLL");
                }
                TypeDefinition ui = plugin.MainModule.Types.Single(t => t.Name == "Plugin");
                MethodDefinition drawUi = ui.Methods.Single(m => m.Name == "OnGUI");
                Check(Calls(drawUi, "Puchatek.FlotsamPlus.WindowLayout", "Fit") &&
                    Calls(drawUi, "Puchatek.FlotsamPlus.WindowAppearance", "Ensure") &&
                    drawUi.Body.ExceptionHandlers.Any(h => h.HandlerType == ExceptionHandlerType.Finally),
                    "The menu projects window bounds and restores GUI state in a finally block");
                Check(drawUi.Body.Instructions.Count(i => i.Operand is MethodReference call &&
                    call.DeclaringType.FullName == "UnityEngine.GUI" && call.Name == "set_skin") == 2 &&
                    drawUi.Body.Instructions.Count(i => i.Operand is MethodReference call &&
                    call.DeclaringType.FullName == "UnityEngine.GUI" && call.Name == "set_matrix") == 2,
                    "The private skin and pixel matrix are installed and restored symmetrically");
                MethodDefinition capture = ui.Methods.Single(m => m.Name == "CapturesWindowInput");
                Check(Reads(capture, ui.FullName, "_resizing") > 0 &&
                    Reads(capture, ui.FullName, "_captureMouseRelease") > 0 &&
                    Calls(ui.Methods.Single(m => m.Name == "BlockGameplayInput"), ui.FullName, "CapturesWindowInput"),
                    "Menu input remains captured when a resize leaves the window rectangle");
                MethodDefinition cleanup = ui.Methods.Single(m => m.Name == "DisposeWindowPreferences");
                MethodDefinition dispose = plugin.MainModule.Types.Single(t => t.Name == "WindowAppearance")
                    .Methods.Single(m => m.Name == "Dispose");
                Check(Calls(ui.Methods.Single(m => m.Name == "OnDestroy"), ui.FullName, "DisposeWindowPreferences") &&
                    Calls(cleanup, "Puchatek.FlotsamPlus.WindowAppearance", "Dispose") &&
                    Calls(dispose, "UnityEngine.Object", "Destroy"),
                    "Unloading saves preferences and destroys the owned skin and textures");
                MethodDefinition resetUi = ui.Methods.Single(m => m.Name == "ResetWindowPreferences");
                Check(!Calls(resetUi, ui.FullName, "ResetShipSettings") &&
                    Calls(resetUi, ui.FullName, "SaveWindowPreferences"),
                    "Emergency window reset saves appearance without resetting ship tuning");
                Check(plugin.MainModule.Types.Any(t => t.Name == "MenuAxisInputPatch" &&
                    t.CustomAttributes.Any(a => a.AttributeType.Name == "HarmonyPatch")),
                    "Scroll/zoom interception is included alongside the button input patch");

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

