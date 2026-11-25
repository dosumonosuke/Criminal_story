using UnityEngine;

namespace CriminalStory
{
    /// <summary>References the story needs from the generated world.</summary>
    public class WorldRefs
    {
        public Vector3 BabyCameraPos;
        public Vector3 BabyLookTarget;
        public Vector3 TownPlayerStart;
        public float TownPlayerYaw;
        public Transform Grandmother;
        public Transform GrandmotherHouse;
    }

    /// <summary>
    /// Builds the whole visible world in code at startup, so the project
    /// does not depend on hand-authored scene YAML:
    ///   - Chapter 0: a hospital room in Rochester, Michigan (at origin)
    ///   - Chapter 1: a Michigan residential block (offset +220 on X)
    /// Large surfaces (lawn, roads, sidewalks, house walls, hospital
    /// floor) use the downloaded CC0 PBR sets in Resources/Textures
    /// (ambientCG, see ASSETS.md); the sky uses the Poly Haven HDRI.
    /// Set dressing and people are still primitive-based placeholders —
    /// the import path for real models is documented in unity/README.md.
    /// </summary>
    public static class WorldBuilder
    {
        public const float TownOffsetX = 220f;

        private static readonly Color White = new Color(0.92f, 0.92f, 0.90f);
        private static readonly Color WallCream = new Color(0.88f, 0.85f, 0.78f);
        private static readonly Color SkinA = new Color(0.96f, 0.76f, 0.60f);
        private static readonly Color SkinB = new Color(0.72f, 0.50f, 0.36f);
        private static readonly Color SkinC = new Color(0.55f, 0.38f, 0.26f);

        public static WorldRefs BuildAll()
        {
            WorldRefs refs = new WorldRefs();
            BuildLightingAndSky();
            BuildHospital(refs);
            BuildTown(refs);
            return refs;
        }

        // ----------------------------------------------------------------
        // Lighting / sky
        // ----------------------------------------------------------------
        private static void BuildLightingAndSky()
        {
            GameObject sunGo = new GameObject("Sun");
            Light sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1.0f, 0.96f, 0.88f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;
            sunGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.60f, 0.68f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.74f, 0.80f, 0.86f);
            RenderSettings.fogStartDistance = 60f;
            RenderSettings.fogEndDistance = 260f;

            // HDRI sky (Poly Haven, CC0). With the built-in pipeline this
            // panoramic skybox is used directly. Under HDRP the sky comes
            // from the Volume (see PostProcessSetup / README) — the same
            // HDRI file, imported as a Cubemap, is assigned there.
            Texture skyTex = Resources.Load<Texture>("Environment/kloppenheim_06_puresky_1k");
            if (skyTex != null)
            {
                Shader skyShader = Shader.Find("Skybox/Panoramic");
                if (skyShader != null)
                {
                    Material skyMat = new Material(skyShader);
                    skyMat.SetTexture("_MainTex", skyTex);
                    if (skyMat.HasProperty("_Exposure"))
                    {
                        skyMat.SetFloat("_Exposure", 1.05f);
                    }
                    RenderSettings.skybox = skyMat;
                }
            }
        }

        // ----------------------------------------------------------------
        // Chapter 0 — hospital room (origin)
        // ----------------------------------------------------------------
        private static void BuildHospital(WorldRefs refs)
        {
            GameObject root = new GameObject("HospitalRoom");

            // Floor: paving-stone PBR set, light tint ( reads as hospital tile )
            BuildUtil.TexturedBox("Floor", root.transform, new Vector3(0f, -0.06f, 0f),
                new Vector3(14f, 0.12f, 11f), "PavingStones036", new Vector2(4f, 3f),
                new Color(0.95f, 0.97f, 0.98f));
            // Ceiling
            BuildUtil.Box("Ceiling", root.transform, new Vector3(0f, 4.02f, 0f),
                new Vector3(14f, 0.12f, 11f), White);
            // Ceiling light panels (emissive)
            BuildUtil.BoxNoCollider("LightPanel1", root.transform, new Vector3(-2.5f, 3.94f, -1f),
                new Vector3(2.4f, 0.06f, 0.9f), new Color(1f, 1f, 0.95f), true);
            BuildUtil.BoxNoCollider("LightPanel2", root.transform, new Vector3(2.5f, 3.94f, -1f),
                new Vector3(2.4f, 0.06f, 0.9f), new Color(1f, 1f, 0.95f), true);
            GameObject roomLightGo = new GameObject("RoomLight");
            roomLightGo.transform.SetParent(root.transform, false);
            roomLightGo.transform.localPosition = new Vector3(0f, 3.4f, -0.5f);
            Light roomLight = roomLightGo.AddComponent<Light>();
            roomLight.type = LightType.Point;
            roomLight.intensity = 1.1f;
            roomLight.range = 14f;
            roomLight.color = new Color(1f, 0.98f, 0.92f);

            // Walls
            BuildUtil.Box("WallBack", root.transform, new Vector3(0f, 2f, -5.56f),
                new Vector3(14f, 4f, 0.12f), WallCream);
            BuildUtil.Box("WallFront", root.transform, new Vector3(0f, 2f, 5.56f),
                new Vector3(14f, 4f, 0.12f), WallCream);
            BuildUtil.Box("WallLeft", root.transform, new Vector3(-7.06f, 2f, 0f),
                new Vector3(0.12f, 4f, 11f), WallCream);
            BuildUtil.Box("WallRight", root.transform, new Vector3(7.06f, 2f, 0f),
                new Vector3(0.12f, 4f, 11f), WallCream);
            // Wainscot stripe for a little institutional detail
            BuildUtil.BoxNoCollider("StripeBack", root.transform, new Vector3(0f, 1.0f, -5.48f),
                new Vector3(14f, 0.5f, 0.03f), new Color(0.55f, 0.68f, 0.66f));

            // Window on the back wall: pale emissive pane + frame + winter light feel
            BuildUtil.BoxNoCollider("WindowPane", root.transform, new Vector3(3.4f, 2.5f, -5.47f),
                new Vector3(2.6f, 1.8f, 0.04f), new Color(0.85f, 0.92f, 1.0f), true);
            BuildUtil.BoxNoCollider("WindowFrameH", root.transform, new Vector3(3.4f, 2.5f, -5.44f),
                new Vector3(2.7f, 0.08f, 0.05f), White);
            BuildUtil.BoxNoCollider("WindowFrameV", root.transform, new Vector3(3.4f, 2.5f, -5.44f),
                new Vector3(0.08f, 1.9f, 0.05f), White);

            // Wall clock
            GameObject clock = BuildUtil.Cylinder("Clock", root.transform,
                new Vector3(-3.6f, 3.0f, -5.44f), new Vector3(0.62f, 0.05f, 0.62f), White);
            clock.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Door (right wall)
            BuildUtil.BoxNoCollider("Door", root.transform, new Vector3(6.97f, 1.15f, 2.6f),
                new Vector3(0.06f, 2.3f, 1.1f), new Color(0.45f, 0.55f, 0.60f));

            // ---- Mother's bed ----
            Vector3 bedPos = new Vector3(-2.1f, 0f, -2.4f);
            BuildUtil.Box("BedFrame", root.transform, bedPos + new Vector3(0f, 0.35f, 0f),
                new Vector3(1.15f, 0.5f, 2.6f), new Color(0.80f, 0.82f, 0.84f));
            BuildUtil.Box("Mattress", root.transform, bedPos + new Vector3(0f, 0.66f, 0f),
                new Vector3(1.05f, 0.18f, 2.5f), White);
            BuildUtil.Box("Pillow", root.transform, bedPos + new Vector3(0f, 0.78f, -1.0f),
                new Vector3(0.7f, 0.12f, 0.5f), White);
            BuildUtil.BoxNoCollider("Blanket", root.transform, bedPos + new Vector3(0f, 0.80f, 0.45f),
                new Vector3(1.06f, 0.10f, 1.5f), new Color(0.62f, 0.72f, 0.85f));

            GameObject motherGo = new GameObject("Mother");
            motherGo.transform.SetParent(root.transform, false);
            motherGo.transform.position = bedPos + new Vector3(0f, 0.78f, 0.1f);
            Npc mother = motherGo.AddComponent<Npc>();
            mother.BuildLying("母親 ダイアン", new Color(0.62f, 0.72f, 0.85f), SkinA);

            // ---- Doctor and nurse (standing, nameplates on) ----
            Npc doctor = MakeNpc(root.transform, "Doctor", new Vector3(-0.5f, 0f, -2.9f), 165f,
                "医師", new Color(0.95f, 0.95f, 0.95f), new Color(0.25f, 0.27f, 0.32f),
                SkinB, new Color(0.10f, 0.10f, 0.10f));
            Npc nurse = MakeNpc(root.transform, "Nurse", new Vector3(1.5f, 0f, -1.7f), 195f,
                "看護師", new Color(0.65f, 0.80f, 0.88f), new Color(0.90f, 0.90f, 0.90f),
                SkinA, new Color(0.45f, 0.30f, 0.15f));
            // A white nurse cap so the roles read at a glance.
            BuildUtil.BoxNoCollider("NurseCap", nurse.transform, new Vector3(0f, 1.90f, 0f),
                new Vector3(0.30f, 0.07f, 0.30f), White);
            // Doctor's clipboard.
            BuildUtil.BoxNoCollider("Clipboard", doctor.transform, new Vector3(0.30f, 0.95f, 0.18f),
                new Vector3(0.26f, 0.36f, 0.03f), new Color(0.55f, 0.42f, 0.28f));

            // ---- Newborn cot (the baby's viewpoint is just above it) ----
            BuildUtil.Box("CotBase", root.transform, new Vector3(0f, 0.45f, 1.6f),
                new Vector3(1.1f, 0.5f, 0.65f), new Color(0.85f, 0.88f, 0.90f));
            BuildUtil.Box("CotMattress", root.transform, new Vector3(0f, 0.74f, 1.6f),
                new Vector3(1.0f, 0.10f, 0.55f), White);
            BuildUtil.BoxNoCollider("CotBlanket", root.transform, new Vector3(0f, 0.80f, 1.6f),
                new Vector3(0.7f, 0.06f, 0.4f), new Color(0.95f, 0.80f, 0.85f));

            // Side table + lamp + cabinet + chair (set dressing)
            BuildUtil.Box("SideTable", root.transform, new Vector3(-3.6f, 0.4f, -2.6f),
                new Vector3(0.7f, 0.8f, 0.6f), new Color(0.60f, 0.48f, 0.36f));
            BuildUtil.Cylinder("LampStand", root.transform, new Vector3(3.9f, 0.8f, -3.4f),
                new Vector3(0.07f, 0.8f, 0.07f), new Color(0.35f, 0.35f, 0.38f));
            BuildUtil.Sphere("LampShade", root.transform, new Vector3(3.9f, 1.75f, -3.4f),
                new Vector3(0.42f, 0.30f, 0.42f), new Color(1f, 0.92f, 0.70f), true);
            BuildUtil.Box("Cabinet", root.transform, new Vector3(5.6f, 0.9f, -4.6f),
                new Vector3(1.8f, 1.8f, 0.8f), new Color(0.75f, 0.78f, 0.80f));
            BuildUtil.Box("ChairSeat", root.transform, new Vector3(1.9f, 0.45f, 0.6f),
                new Vector3(0.55f, 0.08f, 0.55f), new Color(0.50f, 0.58f, 0.66f));
            BuildUtil.Box("ChairBack", root.transform, new Vector3(1.9f, 0.85f, 0.88f),
                new Vector3(0.55f, 0.8f, 0.08f), new Color(0.50f, 0.58f, 0.66f));

            refs.BabyCameraPos = new Vector3(0f, 1.02f, 1.75f);
            refs.BabyLookTarget = new Vector3(-1.1f, 1.15f, -2.4f);
        }

        // ----------------------------------------------------------------
        // Chapter 1 — Michigan residential block (offset on X)
        // ----------------------------------------------------------------
        private static void BuildTown(WorldRefs refs)
        {
            float cx = TownOffsetX;
            GameObject root = new GameObject("TownMichigan");

            // Lawn ground (grass PBR)
            BuildUtil.TexturedBox("Ground", root.transform, new Vector3(cx, -0.06f, 0f),
                new Vector3(130f, 0.12f, 95f), "Grass001", new Vector2(46f, 34f), Color.white);

            // Roads: main street along X, cross street along Z (asphalt PBR)
            BuildUtil.TexturedBox("RoadMain", root.transform, new Vector3(cx, 0.005f, 0f),
                new Vector3(130f, 0.05f, 9.5f), "Asphalt006", new Vector2(40f, 3f), Color.white);
            BuildUtil.TexturedBox("RoadCross", root.transform, new Vector3(cx - 28f, 0.006f, 0f),
                new Vector3(9.5f, 0.05f, 95f), "Asphalt006", new Vector2(3f, 30f), Color.white);

            // Sidewalks (paving PBR)
            BuildUtil.TexturedBox("SidewalkN", root.transform, new Vector3(cx, 0.02f, -6.4f),
                new Vector3(130f, 0.06f, 2.6f), "PavingStones036", new Vector2(44f, 1f), Color.white);
            BuildUtil.TexturedBox("SidewalkS", root.transform, new Vector3(cx, 0.02f, 6.4f),
                new Vector3(130f, 0.06f, 2.6f), "PavingStones036", new Vector2(44f, 1f), Color.white);

            // Centre dashes + crosswalk stripes
            Color lineWhite = new Color(0.92f, 0.92f, 0.90f);
            for (int i = -8; i <= 8; i++)
            {
                if (Mathf.Abs(i) <= 1) continue; // gap at the cross street
                BuildUtil.BoxNoCollider("Dash", root.transform, new Vector3(cx + i * 7.5f, 0.045f, 0f),
                    new Vector3(3.2f, 0.012f, 0.28f), lineWhite);
            }
            for (int i = -1; i <= 1; i++)
            {
                BuildUtil.BoxNoCollider("Crosswalk", root.transform,
                    new Vector3(cx - 28f + i * 2.2f, 0.045f, -3.4f),
                    new Vector3(1.2f, 0.012f, 5.6f), lineWhite);
                BuildUtil.BoxNoCollider("Crosswalk", root.transform,
                    new Vector3(cx - 28f + i * 2.2f, 0.045f, 3.4f),
                    new Vector3(1.2f, 0.012f, 5.6f), lineWhite);
            }

            // Houses: north row faces the road (front toward +Z),
            // south row faces -Z. Walls use the brick PBR set with tints,
            // a few use painted colours for variety.
            Color[] houseTints =
            {
                new Color(1.00f, 0.96f, 0.90f), new Color(0.92f, 0.84f, 0.76f),
                new Color(0.85f, 0.90f, 0.95f), new Color(1.00f, 0.90f, 0.82f),
                new Color(0.88f, 0.94f, 0.86f), new Color(0.96f, 0.88f, 0.94f),
            };
            float[] northX = { -46f, -34f, -12f, 2f, 16f, 30f, 44f };
            for (int i = 0; i < northX.Length; i++)
            {
                BuildHouse(root.transform, new Vector3(cx + northX[i], 0f, -15.5f),
                    0f, houseTints[i % houseTints.Length], i);
            }
            float[] southX = { -40f, -14f, 8f, 24f, 40f };
            for (int i = 0; i < southX.Length; i++)
            {
                BuildHouse(root.transform, new Vector3(cx + southX[i], 0f, 15.5f),
                    180f, houseTints[(i + 3) % houseTints.Length], i + 7);
            }

            // Grandmother's house: the first north-row house nearest spawn,
            // highlighted by the story (golden marker + grandmother on porch).
            Vector3 grandmaPorch = new Vector3(cx - 12f, 0f, -11.2f);

            // Grocery store with an English sign (era-appropriate, fictional)
            BuildStore(root.transform, new Vector3(cx + 34f, 0f, -17f));

            // Trees
            Vector3[] treeSpots =
            {
                new Vector3(cx - 52f, 0f, -10f), new Vector3(cx - 20f, 0f, -10.5f),
                new Vector3(cx - 4f, 0f, 10.5f), new Vector3(cx + 10f, 0f, -10.5f),
                new Vector3(cx + 22f, 0f, 10.5f), new Vector3(cx + 48f, 0f, -10f),
                new Vector3(cx - 38f, 0f, 11f), new Vector3(cx + 52f, 0f, 11f),
                new Vector3(cx - 58f, 0f, 14f), new Vector3(cx + 58f, 0f, -14f),
            };
            for (int i = 0; i < treeSpots.Length; i++)
            {
                BuildTree(root.transform, treeSpots[i], 0.9f + (i % 4) * 0.12f);
            }

            // Parked cars
            BuildCar(root.transform, new Vector3(cx - 18f, 0f, 3.4f), 0f, new Color(0.55f, 0.18f, 0.15f));
            BuildCar(root.transform, new Vector3(cx + 6f, 0f, -3.4f), 180f, new Color(0.20f, 0.32f, 0.55f));
            BuildCar(root.transform, new Vector3(cx + 26f, 0f, 3.4f), 0f, new Color(0.75f, 0.72f, 0.66f));
            BuildCar(root.transform, new Vector3(cx - 44f, 0f, -3.4f), 180f, new Color(0.25f, 0.40f, 0.28f));
            BuildCar(root.transform, new Vector3(cx + 44f, 0f, -10.8f), 0f, new Color(0.60f, 0.50f, 0.22f));

            // Street lights along the main road
            for (int i = -3; i <= 3; i++)
            {
                float z = (i % 2 == 0) ? -5.6f : 5.6f;
                BuildStreetLight(root.transform, new Vector3(cx + i * 16f, 0f, z));
            }

            // Traffic light at the cross-street corner
            BuildTrafficLight(root.transform, new Vector3(cx - 22.6f, 0f, -6.2f));

            // Mailboxes + a white picket fence in front of one house
            for (int i = 0; i < 3; i++)
            {
                BuildMailbox(root.transform, new Vector3(cx - 20f + i * 14f, 0f, -7.6f));
            }
            for (int i = 0; i < 9; i++)
            {
                BuildUtil.Box("Picket", root.transform,
                    new Vector3(cx - 16.5f + i * 0.85f, 0.45f, -10.6f),
                    new Vector3(0.14f, 0.9f, 0.05f), White);
            }
            BuildUtil.Box("PicketRail", root.transform, new Vector3(cx - 13.1f, 0.55f, -10.62f),
                new Vector3(7.4f, 0.09f, 0.04f), White);

            // ---- People ----
            // Grandmother on the porch (objective target) + golden marker.
            Npc grandma = MakeNpc(root.transform, "Grandmother", grandmaPorch, 180f,
                "祖母", new Color(0.45f, 0.38f, 0.55f), new Color(0.30f, 0.28f, 0.34f),
                SkinA, new Color(0.85f, 0.83f, 0.80f));
            refs.Grandmother = grandma.transform;

            GameObject markerGo = new GameObject("ObjectiveMarker");
            markerGo.transform.SetParent(grandma.transform, false);
            markerGo.transform.localPosition = new Vector3(0f, 2.75f, 0f);
            GameObject markerMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            markerMesh.name = "MarkerMesh";
            markerMesh.transform.SetParent(markerGo.transform, false);
            markerMesh.transform.localScale = new Vector3(0.30f, 0.55f, 0.30f);
            MeshRenderer markerRenderer = markerMesh.GetComponent<MeshRenderer>();
            markerRenderer.sharedMaterial = MatUtil.GetTransparent(
                new Color(1.0f, 0.80f, 0.25f), 0.85f);
            Object.Destroy(markerMesh.GetComponent<Collider>());
            markerGo.AddComponent<MarkerBob>();

            // Grandfather by the fence, Keith (child) next to grandmother.
            MakeNpc(root.transform, "Grandfather", new Vector3(cx - 17.5f, 0f, -10.9f), 150f,
                "祖父", new Color(0.32f, 0.30f, 0.28f), new Color(0.22f, 0.22f, 0.24f),
                SkinB, new Color(0.75f, 0.73f, 0.70f));
            MakeNpc(root.transform, "Keith", grandmaPorch + new Vector3(1.3f, 0f, 0.4f), 190f,
                "キース（兄）", new Color(0.70f, 0.30f, 0.22f), new Color(0.25f, 0.30f, 0.45f),
                SkinA, new Color(0.25f, 0.16f, 0.10f), 0.62f);

            // Walking neighbours (life in the town; distinct palettes).
            Npc walker1 = MakeNpc(root.transform, "Neighbour1", Vector3.zero, 0f,
                "近所の人", new Color(0.25f, 0.45f, 0.60f), new Color(0.35f, 0.32f, 0.28f),
                SkinC, new Color(0.08f, 0.08f, 0.08f));
            walker1.SetWaypoints(new[]
            {
                new Vector3(cx - 50f, 0f, -6.4f), new Vector3(cx + 50f, 0f, -6.4f),
            }, 1.2f);
            Npc walker2 = MakeNpc(root.transform, "Neighbour2", Vector3.zero, 0f,
                "近所の人", new Color(0.62f, 0.52f, 0.20f), new Color(0.40f, 0.36f, 0.30f),
                SkinB, new Color(0.35f, 0.22f, 0.12f));
            walker2.SetWaypoints(new[]
            {
                new Vector3(cx + 40f, 0f, 6.4f), new Vector3(cx - 40f, 0f, 6.4f),
            }, 1.0f);
            Npc walker3 = MakeNpc(root.transform, "Neighbour3", Vector3.zero, 0f,
                "郵便配達員", new Color(0.30f, 0.38f, 0.62f), new Color(0.28f, 0.30f, 0.40f),
                SkinA, new Color(0.55f, 0.40f, 0.22f));
            walker3.SetWaypoints(new[]
            {
                new Vector3(cx - 28f, 0f, -14f), new Vector3(cx - 28f, 0f, 14f),
                new Vector3(cx - 20f, 0f, 14f), new Vector3(cx - 20f, 0f, -14f),
            }, 1.35f);

            refs.TownPlayerStart = new Vector3(cx - 26f, 0.1f, 5.2f);
            refs.TownPlayerYaw = 139f; // face toward the grandmother's house (north row)
            refs.GrandmotherHouse = null;
        }

        // ----------------------------------------------------------------
        // Builders for repeated objects
        // ----------------------------------------------------------------
        private static Npc MakeNpc(Transform parent, string objectName, Vector3 pos,
            float yawDegrees, string displayName, Color clothes, Color pants,
            Color skin, Color hair, float scale = 1.0f)
        {
            GameObject go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
            Npc npc = go.AddComponent<Npc>();
            npc.Build(displayName, clothes, pants, skin, hair, scale);
            return npc;
        }

        private static void BuildHouse(Transform parent, Vector3 pos, float yaw,
            Color tint, int variant)
        {
            GameObject house = new GameObject("House");
            house.transform.SetParent(parent, false);
            house.transform.position = pos;
            house.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            float w = 7.5f + (variant % 3) * 0.8f;
            float d = 6.5f;
            float h = 3.3f;
            // Walls with brick PBR (tinted per house for variety)
            BuildUtil.TexturedBox("Walls", house.transform, new Vector3(0f, h * 0.5f, 0f),
                new Vector3(w, h, d), "Bricks056", new Vector2(3f, 1.4f), tint);
            // Roof: two slanted slabs + gable ends
            Color roofColor = new Color(0.32f, 0.26f, 0.24f);
            GameObject roofL = BuildUtil.Box("RoofL", house.transform,
                new Vector3(-w * 0.26f, h + 0.85f, 0f), new Vector3(w * 0.62f, 0.14f, d + 0.7f), roofColor);
            roofL.transform.localRotation = Quaternion.Euler(0f, 0f, 38f);
            GameObject roofR = BuildUtil.Box("RoofR", house.transform,
                new Vector3(w * 0.26f, h + 0.85f, 0f), new Vector3(w * 0.62f, 0.14f, d + 0.7f), roofColor);
            roofR.transform.localRotation = Quaternion.Euler(0f, 0f, -38f);
            BuildUtil.Box("Chimney", house.transform, new Vector3(w * 0.25f, h + 1.5f, 0.5f),
                new Vector3(0.6f, 1.3f, 0.6f), new Color(0.55f, 0.35f, 0.30f));
            // Door + step (front is +Z in local space)
            BuildUtil.BoxNoCollider("Door", house.transform, new Vector3(-w * 0.22f, 1.05f, d * 0.5f + 0.04f),
                new Vector3(1.0f, 2.1f, 0.08f), new Color(0.40f, 0.25f, 0.16f));
            BuildUtil.Box("Step", house.transform, new Vector3(-w * 0.22f, 0.12f, d * 0.5f + 0.5f),
                new Vector3(1.6f, 0.24f, 1.1f), new Color(0.70f, 0.70f, 0.68f));
            // Windows (front)
            Color windowColor = new Color(0.65f, 0.78f, 0.88f);
            BuildUtil.BoxNoCollider("Window1", house.transform, new Vector3(w * 0.22f, 1.8f, d * 0.5f + 0.04f),
                new Vector3(1.3f, 1.1f, 0.08f), windowColor, true);
            BuildUtil.BoxNoCollider("Window2", house.transform, new Vector3(-w * 0.02f, 1.8f, d * 0.5f + 0.04f),
                new Vector3(1.1f, 1.1f, 0.08f), windowColor, true);
            // Porch slab
            BuildUtil.Box("Porch", house.transform, new Vector3(0f, 0.09f, d * 0.5f + 1.1f),
                new Vector3(w * 0.8f, 0.18f, 2.0f), new Color(0.72f, 0.70f, 0.66f));
        }

        private static void BuildStore(Transform parent, Vector3 pos)
        {
            GameObject store = new GameObject("GroceryStore");
            store.transform.SetParent(parent, false);
            store.transform.position = pos;

            BuildUtil.TexturedBox("StoreWalls", store.transform, new Vector3(0f, 2.2f, 0f),
                new Vector3(13f, 4.4f, 8f), "Bricks056", new Vector2(5f, 1.8f),
                new Color(0.98f, 0.94f, 0.88f));
            BuildUtil.Box("StoreRoof", store.transform, new Vector3(0f, 4.6f, 0f),
                new Vector3(13.6f, 0.4f, 8.6f), new Color(0.30f, 0.28f, 0.28f));
            // Shopfront glass + door (front faces +Z, toward the road)
            BuildUtil.BoxNoCollider("ShopGlass", store.transform, new Vector3(-2.5f, 1.5f, 4.04f),
                new Vector3(6.5f, 2.2f, 0.08f), new Color(0.55f, 0.70f, 0.80f), true);
            BuildUtil.BoxNoCollider("ShopDoor", store.transform, new Vector3(3.4f, 1.25f, 4.04f),
                new Vector3(1.4f, 2.5f, 0.08f), new Color(0.35f, 0.30f, 0.26f));
            // Sign board + 3D text (ASCII so it renders with any font)
            BuildUtil.BoxNoCollider("SignBoard", store.transform, new Vector3(0f, 3.9f, 4.15f),
                new Vector3(8.5f, 1.1f, 0.18f), new Color(0.20f, 0.35f, 0.55f));
            GameObject signGo = new GameObject("SignText");
            signGo.transform.SetParent(store.transform, false);
            signGo.transform.localPosition = new Vector3(0f, 3.9f, 4.28f);
            TextMesh sign = signGo.AddComponent<TextMesh>();
            sign.text = "GROCERY";
            sign.fontSize = 64;
            sign.characterSize = 0.55f;
            sign.anchor = TextAnchor.MiddleCenter;
            sign.alignment = TextAlignment.Center;
            sign.color = new Color(1f, 0.95f, 0.80f);
            Font font = BuildUtil.DefaultFont();
            if (font != null)
            {
                sign.font = font;
                signGo.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            }
        }

        private static void BuildTree(Transform parent, Vector3 pos, float scale)
        {
            GameObject tree = new GameObject("Tree");
            tree.transform.SetParent(parent, false);
            tree.transform.position = pos;
            tree.transform.localScale = Vector3.one * scale;
            BuildUtil.Cylinder("Trunk", tree.transform, new Vector3(0f, 1.1f, 0f),
                new Vector3(0.22f, 1.1f, 0.22f), new Color(0.40f, 0.30f, 0.20f));
            Color leaf = new Color(0.30f, 0.52f, 0.26f);
            BuildUtil.Sphere("Crown1", tree.transform, new Vector3(0f, 3.0f, 0f),
                new Vector3(2.4f, 2.2f, 2.4f), leaf);
            BuildUtil.Sphere("Crown2", tree.transform, new Vector3(0.8f, 2.4f, 0.4f),
                new Vector3(1.6f, 1.5f, 1.6f), leaf);
            BuildUtil.Sphere("Crown3", tree.transform, new Vector3(-0.8f, 2.5f, -0.3f),
                new Vector3(1.5f, 1.4f, 1.5f), leaf);
        }

        private static void BuildCar(Transform parent, Vector3 pos, float yaw, Color bodyColor)
        {
            GameObject car = new GameObject("Car");
            car.transform.SetParent(parent, false);
            car.transform.position = pos;
            car.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            BuildUtil.Box("CarBody", car.transform, new Vector3(0f, 0.55f, 0f),
                new Vector3(4.2f, 0.65f, 1.8f), bodyColor);
            BuildUtil.Box("CarCabin", car.transform, new Vector3(-0.2f, 1.12f, 0f),
                new Vector3(2.2f, 0.55f, 1.6f), new Color(0.55f, 0.68f, 0.78f), true);
            Color tyre = new Color(0.10f, 0.10f, 0.10f);
            Vector3[] wheels =
            {
                new Vector3(1.45f, 0.32f, 0.92f), new Vector3(1.45f, 0.32f, -0.92f),
                new Vector3(-1.45f, 0.32f, 0.92f), new Vector3(-1.45f, 0.32f, -0.92f),
            };
            for (int i = 0; i < wheels.Length; i++)
            {
                GameObject wheel = BuildUtil.Cylinder("Wheel", car.transform, wheels[i],
                    new Vector3(0.32f, 0.12f, 0.32f), tyre);
                wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            BuildUtil.BoxNoCollider("Headlights", car.transform, new Vector3(2.12f, 0.62f, 0f),
                new Vector3(0.05f, 0.22f, 1.4f), new Color(1f, 0.95f, 0.75f), true);
        }

        private static void BuildStreetLight(Transform parent, Vector3 pos)
        {
            GameObject lamp = new GameObject("StreetLight");
            lamp.transform.SetParent(parent, false);
            lamp.transform.position = pos;
            Color pole = new Color(0.25f, 0.27f, 0.30f);
            BuildUtil.Cylinder("Pole", lamp.transform, new Vector3(0f, 2.6f, 0f),
                new Vector3(0.09f, 2.6f, 0.09f), pole);
            float towardRoad = pos.z > 0f ? -1f : 1f;
            BuildUtil.Box("Arm", lamp.transform, new Vector3(0f, 5.15f, towardRoad * 0.7f),
                new Vector3(0.12f, 0.12f, 1.6f), pole);
            BuildUtil.Sphere("LampHead", lamp.transform, new Vector3(0f, 5.05f, towardRoad * 1.4f),
                new Vector3(0.34f, 0.22f, 0.34f), new Color(1f, 0.93f, 0.70f), true);
        }

        private static void BuildTrafficLight(Transform parent, Vector3 pos)
        {
            GameObject tl = new GameObject("TrafficLight");
            tl.transform.SetParent(parent, false);
            tl.transform.position = pos;
            BuildUtil.Cylinder("Pole", tl.transform, new Vector3(0f, 2.4f, 0f),
                new Vector3(0.10f, 2.4f, 0.10f), new Color(0.22f, 0.22f, 0.24f));
            BuildUtil.Box("SignalBox", tl.transform, new Vector3(0f, 4.6f, 0f),
                new Vector3(0.42f, 1.15f, 0.30f), new Color(0.15f, 0.15f, 0.16f));
            BuildUtil.Sphere("LightRed", tl.transform, new Vector3(0f, 4.95f, 0.17f),
                new Vector3(0.20f, 0.20f, 0.10f), new Color(0.85f, 0.15f, 0.12f), true);
            BuildUtil.Sphere("LightYellow", tl.transform, new Vector3(0f, 4.60f, 0.17f),
                new Vector3(0.20f, 0.20f, 0.10f), new Color(0.45f, 0.35f, 0.10f));
            BuildUtil.Sphere("LightGreen", tl.transform, new Vector3(0f, 4.25f, 0.17f),
                new Vector3(0.20f, 0.20f, 0.10f), new Color(0.15f, 0.70f, 0.30f), true);
        }

        private static void BuildMailbox(Transform parent, Vector3 pos)
        {
            GameObject mb = new GameObject("Mailbox");
            mb.transform.SetParent(parent, false);
            mb.transform.position = pos;
            BuildUtil.Box("Post", mb.transform, new Vector3(0f, 0.5f, 0f),
                new Vector3(0.12f, 1.0f, 0.12f), new Color(0.45f, 0.35f, 0.25f));
            BuildUtil.Box("BoxBody", mb.transform, new Vector3(0f, 1.15f, 0f),
                new Vector3(0.55f, 0.35f, 0.35f), new Color(0.30f, 0.38f, 0.55f));
            BuildUtil.BoxNoCollider("Flag", mb.transform, new Vector3(0.30f, 1.30f, 0f),
                new Vector3(0.05f, 0.28f, 0.08f), new Color(0.80f, 0.20f, 0.15f));
        }
    }
}
