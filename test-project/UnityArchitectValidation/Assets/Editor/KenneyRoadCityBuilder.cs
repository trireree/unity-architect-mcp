#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Antigravity.City
{
    public static class KenneyRoadCityBuilder
    {
        [MenuItem("Antigravity/Build Complete Kenney Road City")]
        public static void BuildFullCity()
        {
            // 1. Clean previous city objects
            string[] oldRoots = new string[] { "Kenney_OpenWorld_City", "City_Prototype_Root", "FPS_Playground_Root" };
            foreach (var r in oldRoots)
            {
                var obj = GameObject.Find(r);
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }

            var root = new GameObject("Kenney_OpenWorld_City");
            Undo.RegisterCreatedObjectUndo(root, "Build Kenney Road City");

            // 2. Single Continuous Seamless Ground Collider (600m x 600m)
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "City_Ground_Base";
            ground.transform.localScale = new Vector3(60f, 1f, 60f); // 600m x 600m
            ground.transform.position = Vector3.zero;
            ground.transform.SetParent(root.transform);

            // Solid 1-piece BoxCollider for 100% seamless ground walking
            var oldPCol = ground.GetComponent<Collider>();
            if (oldPCol != null) UnityEngine.Object.DestroyImmediate(oldPCol);
            var gBox = ground.AddComponent<BoxCollider>();
            gBox.size = new Vector3(600f, 2.0f, 600f);
            gBox.center = new Vector3(0f, -1.0f, 0f);

            var groundMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Ground.mat");
            if (groundMat != null) ground.GetComponent<Renderer>().sharedMaterial = groundMat;

            // Asset Folder Paths
            string roadFolder = "Assets/3d assetler/kenney_city-kit-roads/Models/FBX format/";
            string subFolder = "Assets/3d assetler/kenney_city-kit-suburban_20/Models/FBX format/";
            string comFolder = "Assets/3d assetler/kenney_city-kit-commercial_2.1/Models/FBX format/";
            string indFolder = "Assets/3d assetler/kenney_city-kit-industrial_1.0/Models/FBX format/";
            string carFolder = "Assets/3d assetler/kenney_car-kit/Models/FBX format/";
            string charFolder = "Assets/3d assetler/kenney_mini-characters/Models/FBX format/";

            GameObject LoadFbx(string relPath)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(relPath);
                if (prefab != null)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    instance.name = prefab.name;
                    return instance;
                }
                return null;
            }

            // 3. ROAD NETWORK (Visual Tiles Over Seamless Ground)
            var roadsRoot = new GameObject("Road_Network");
            roadsRoot.transform.SetParent(root.transform);

            float tileSize = 12f;
            float[] gridLines = new float[] { -120f, 0f, 120f };
            float cityMin = -240f;
            float cityMax = 240f;

            var placedRoadPositions = new HashSet<Vector2Int>();

            System.Action<float, float, string, float> placeRoadTile = (x, z, fbxName, rotY) =>
            {
                int gx = Mathf.RoundToInt(x / tileSize);
                int gz = Mathf.RoundToInt(z / tileSize);
                var key = new Vector2Int(gx, gz);
                if (placedRoadPositions.Contains(key)) return;
                placedRoadPositions.Add(key);

                var tile = LoadFbx(roadFolder + fbxName);
                if (tile != null)
                {
                    tile.transform.position = new Vector3(x, 0.01f, z);
                    tile.transform.localScale = Vector3.one * tileSize;
                    tile.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
                    tile.transform.SetParent(roadsRoot.transform);

                    // Remove any internal colliders from road visual meshes to keep ground 100% seamless
                    var existingCols = tile.GetComponentsInChildren<Collider>();
                    foreach (var c in existingCols) UnityEngine.Object.DestroyImmediate(c);
                }
            };

            // A) Intersections
            foreach (float gx in gridLines)
            {
                foreach (float gz in gridLines)
                {
                    placeRoadTile(gx, gz, "road-crossroad.fbx", 0f);

                    var tl = LoadFbx(roadFolder + "traffic-light.fbx");
                    if (tl != null)
                    {
                        tl.transform.position = new Vector3(gx + 5.5f, 0.05f, gz + 5.5f);
                        tl.transform.localScale = Vector3.one * 10f;
                        tl.transform.SetParent(roadsRoot.transform);
                    }
                }
            }

            // B) Straight Avenue Segments
            foreach (float gz in gridLines)
            {
                for (float x = cityMin; x <= cityMax; x += tileSize)
                {
                    if (Mathf.Abs(x - (-120f)) < 2f || Mathf.Abs(x - 0f) < 2f || Mathf.Abs(x - 120f) < 2f) continue;
                    string rFbx = (Mathf.Abs(x) % (tileSize * 4f) < 1f) ? "road-crossing.fbx" : "road-straight.fbx";
                    placeRoadTile(x, gz, rFbx, 90f);

                    if (Mathf.Abs(x) % (tileSize * 3f) < 1f)
                    {
                        var sl = LoadFbx(roadFolder + "light-curved.fbx");
                        if (sl != null)
                        {
                            sl.transform.position = new Vector3(x, 0.05f, gz + 5.2f);
                            sl.transform.localScale = Vector3.one * 10f;
                            sl.transform.SetParent(roadsRoot.transform);
                        }
                    }
                }
            }

            foreach (float gx in gridLines)
            {
                for (float z = cityMin; z <= cityMax; z += tileSize)
                {
                    if (Mathf.Abs(z - (-120f)) < 2f || Mathf.Abs(z - 0f) < 2f || Mathf.Abs(z - 120f) < 2f) continue;
                    string rFbx = (Mathf.Abs(z) % (tileSize * 4f) < 1f) ? "road-crossing.fbx" : "road-straight.fbx";
                    placeRoadTile(gx, z, rFbx, 0f);
                }
            }

            // 4. DISTRICT BUILDINGS (SOLID BOX COLLIDERS)
            var buildingsRoot = new GameObject("Buildings_Districts");
            buildingsRoot.transform.SetParent(root.transform);

            System.Action<GameObject> ensureSolidCollider = (go) =>
            {
                var renderers = go.GetComponentsInChildren<MeshRenderer>();
                if (renderers.Length > 0)
                {
                    var bounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

                    var boxCol = go.AddComponent<BoxCollider>();
                    boxCol.center = go.transform.InverseTransformPoint(bounds.center);
                    boxCol.size = bounds.size;
                }
            };

            // A) SUBURBAN DISTRICT (West)
            var subRoot = new GameObject("District_Suburban");
            subRoot.transform.SetParent(buildingsRoot.transform);
            string[] subModels = new string[] {
                "building-type-a.fbx", "building-type-b.fbx", "building-type-c.fbx",
                "building-type-d.fbx", "building-type-e.fbx", "building-type-f.fbx",
                "building-type-g.fbx", "building-type-h.fbx"
            };

            int bIdx = 0;
            float[] subXCenters = new float[] { -180f, -60f };
            float[] subZCenters = new float[] { -180f, -60f, 60f, 180f };

            foreach (float cx in subXCenters)
            {
                foreach (float cz in subZCenters)
                {
                    for (int dx = -1; dx <= 1; dx += 2)
                    {
                        for (int dz = -1; dz <= 1; dz += 2)
                        {
                            var b = LoadFbx(subFolder + subModels[(bIdx++) % subModels.Length]);
                            if (b != null)
                            {
                                b.transform.position = new Vector3(cx + dx * 24f, 0f, cz + dz * 24f);
                                b.transform.localScale = Vector3.one * 16f;
                                b.transform.rotation = Quaternion.Euler(0f, (dx > 0 ? 180f : 0f), 0f);
                                ensureSolidCollider(b);
                                b.transform.SetParent(subRoot.transform);
                            }
                        }
                    }
                }
            }

            // B) COMMERCIAL DOWNTOWN DISTRICT (Center)
            var comRoot = new GameObject("District_Commercial_Downtown");
            comRoot.transform.SetParent(buildingsRoot.transform);
            string[] comModels = new string[] {
                "building-a.fbx", "building-b.fbx", "building-c.fbx", "building-d.fbx",
                "building-e.fbx", "building-f.fbx", "building-g.fbx", "building-h.fbx",
                "building-i.fbx", "building-j.fbx"
            };

            float[] comXCenters = new float[] { 60f };
            float[] comZCenters = new float[] { -180f, -60f, 60f, 180f };

            foreach (float cx in comXCenters)
            {
                foreach (float cz in comZCenters)
                {
                    for (int dx = -1; dx <= 1; dx += 2)
                    {
                        for (int dz = -1; dz <= 1; dz += 2)
                        {
                            var b = LoadFbx(comFolder + comModels[(bIdx++) % comModels.Length]);
                            if (b != null)
                            {
                                b.transform.position = new Vector3(cx + dx * 22f, 0f, cz + dz * 22f);
                                b.transform.localScale = Vector3.one * 18f;
                                b.transform.rotation = Quaternion.Euler(0f, (dz > 0 ? 90f : -90f), 0f);
                                ensureSolidCollider(b);
                                b.transform.SetParent(comRoot.transform);
                            }
                        }
                    }
                }
            }

            // C) INDUSTRIAL DISTRICT (East)
            var indRoot = new GameObject("District_Industrial");
            indRoot.transform.SetParent(buildingsRoot.transform);
            string[] indModels = new string[] {
                "building-a.fbx", "building-b.fbx", "building-c.fbx",
                "building-d.fbx", "building-e.fbx", "building-f.fbx",
                "building-g.fbx", "building-h.fbx"
            };

            float[] indXCenters = new float[] { 180f };
            float[] indZCenters = new float[] { -180f, -60f, 60f, 180f };

            foreach (float cx in indXCenters)
            {
                foreach (float cz in indZCenters)
                {
                    for (int dx = -1; dx <= 1; dx += 2)
                    {
                        for (int dz = -1; dz <= 1; dz += 2)
                        {
                            var b = LoadFbx(indFolder + indModels[(bIdx++) % indModels.Length]);
                            if (b != null)
                            {
                                b.transform.position = new Vector3(cx + dx * 26f, 0f, cz + dz * 26f);
                                b.transform.localScale = Vector3.one * 20f;
                                b.transform.rotation = Quaternion.Euler(0f, (dx > 0 ? 180f : 0f), 0f);
                                ensureSolidCollider(b);
                                b.transform.SetParent(indRoot.transform);
                            }
                        }
                    }
                }
            }

            // 5. VEHICLES
            var vehiclesRoot = new GameObject("Vehicles_Root");
            vehiclesRoot.transform.SetParent(root.transform);

            GameObject SpawnVehicle(Vector3 pos, string carFbx, bool isParked, Vector3 laneDir, float rotY = 0f)
            {
                var carContainer = new GameObject("Car_" + carFbx.Replace(".fbx", ""));
                carContainer.transform.position = new Vector3(pos.x, 0.05f, pos.z);
                carContainer.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
                carContainer.transform.SetParent(vehiclesRoot.transform);

                var carVisual = LoadFbx(carFolder + carFbx);
                if (carVisual != null)
                {
                    var innerCols = carVisual.GetComponentsInChildren<Collider>();
                    foreach (var c in innerCols) UnityEngine.Object.DestroyImmediate(c);

                    carVisual.transform.SetParent(carContainer.transform);
                    carVisual.transform.localPosition = Vector3.zero;
                    carVisual.transform.localRotation = Quaternion.identity;
                    carVisual.transform.localScale = Vector3.one * 2.0f;
                }

                var boxCol = carContainer.AddComponent<BoxCollider>();
                boxCol.size = new Vector3(2.0f, 1.2f, 4.2f);
                boxCol.center = new Vector3(0f, 0.65f, 0f);

                var veh = carContainer.AddComponent<Vehicle>();
                veh.vehicleName = carFbx.Replace(".fbx", "").Replace("-", " ");
                veh.isParked = isParked;
                veh.isEngineRunning = false;

                var vAudio = carContainer.AddComponent<VehicleAudio>();
                vAudio.AutoLoadAudioClips();

                if (!isParked)
                {
                    var traffic = carContainer.AddComponent<TrafficVehicleController>();
                    traffic.targetLaneDirection = laneDir;
                    traffic.driveSpeed = UnityEngine.Random.Range(8f, 10f);
                }

                return carContainer;
            }

            // Parked Stealable Cars
            SpawnVehicle(new Vector3(5.5f, 0f, 15f), "police.fbx", true, Vector3.forward, 0f);
            SpawnVehicle(new Vector3(-5.5f, 0f, -25f), "taxi.fbx", true, Vector3.back, 180f);
            SpawnVehicle(new Vector3(-125.5f, 0f, 35f), "sedan-sports.fbx", true, Vector3.forward, 0f);
            SpawnVehicle(new Vector3(125.5f, 0f, -45f), "truck.fbx", true, Vector3.back, 180f);
            SpawnVehicle(new Vector3(-55f, 0f, 125.5f), "suv-luxury.fbx", true, Vector3.right, 90f);
            SpawnVehicle(new Vector3(65f, 0f, -125.5f), "van.fbx", true, Vector3.left, -90f);
            SpawnVehicle(new Vector3(-115f, 0f, -125.5f), "hatchback-sports.fbx", true, Vector3.left, -90f);
            SpawnVehicle(new Vector3(175f, 0f, 5.5f), "ambulance.fbx", true, Vector3.right, 90f);

            // Traffic AI Cars
            SpawnVehicle(new Vector3(2.5f, 0f, 50f), "sedan.fbx", false, Vector3.forward, 0f);
            SpawnVehicle(new Vector3(2.5f, 0f, -90f), "suv-luxury.fbx", false, Vector3.forward, 0f);
            SpawnVehicle(new Vector3(-2.5f, 0f, 140f), "van.fbx", false, Vector3.back, 180f);
            SpawnVehicle(new Vector3(-2.5f, 0f, -170f), "taxi.fbx", false, Vector3.back, 180f);
            SpawnVehicle(new Vector3(70f, 0f, 2.5f), "sedan-sports.fbx", false, Vector3.right, 90f);
            SpawnVehicle(new Vector3(-70f, 0f, -2.5f), "truck.fbx", false, Vector3.left, -90f);

            // 6. PEDESTRIAN NPCS
            var pedsRoot = new GameObject("Pedestrians_Root");
            pedsRoot.transform.SetParent(root.transform);

            string[] charModels = new string[] {
                "character-male-a.fbx", "character-male-b.fbx", "character-male-c.fbx",
                "character-male-d.fbx", "character-female-a.fbx", "character-female-b.fbx",
                "character-female-c.fbx", "character-female-d.fbx"
            };

            for (int i = 0; i < 24; i++)
            {
                var charFbx = charModels[i % charModels.Length];
                var pedVisual = LoadFbx(charFolder + charFbx);
                var pedGo = new GameObject("Pedestrian_" + (i + 1));
                float px = ((i % 6) * 40f) - 100f + 6.2f;
                float pz = ((i / 6) * 50f) - 75f;
                pedGo.transform.position = new Vector3(px, 0.2f, pz);
                pedGo.transform.SetParent(pedsRoot.transform);

                if (pedVisual != null)
                {
                    var innerCols = pedVisual.GetComponentsInChildren<Collider>();
                    foreach (var c in innerCols) UnityEngine.Object.DestroyImmediate(c);

                    pedVisual.transform.SetParent(pedGo.transform);
                    pedVisual.transform.localPosition = Vector3.zero;
                    pedVisual.transform.localScale = Vector3.one * 1.8f;
                }

                var col = pedGo.AddComponent<CapsuleCollider>();
                col.height = 1.8f;
                col.radius = 0.45f;
                col.center = new Vector3(0f, 0.9f, 0f);

                pedGo.AddComponent<PedestrianAI>();
            }

            // 7. FPS PLAYER
            var playerGo = new GameObject("FPS_Player");
            playerGo.tag = "Player";
            playerGo.transform.position = new Vector3(8.0f, 1.0f, 8.0f);

            var cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.45f;
            cc.center = new Vector3(0f, 0f, 0f);

            var fpsPlayer = playerGo.AddComponent<FPSPlayer>();
            playerGo.transform.SetParent(root.transform);

            var existingCam = GameObject.FindWithTag("MainCamera");
            var camGo = existingCam != null ? existingCam : new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(playerGo.transform);
            camGo.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            camGo.transform.localRotation = Quaternion.identity;

            fpsPlayer.playerCamera = camGo.GetComponent<Camera>();

            // 8. HUD & LIGHTING
            var hudGo = new GameObject("HUD_Manager");
            hudGo.AddComponent<HUDController>();
            hudGo.transform.SetParent(root.transform);

            var sun = GameObject.Find("Sun_DirectionalLight") ?? GameObject.Find("Directional Light");
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
                var l = sun.GetComponent<Light>();
                if (l != null)
                {
                    l.color = new Color(1f, 0.96f, 0.90f);
                    l.intensity = 1.4f;
                }
            }

            Selection.activeGameObject = playerGo;
            SceneView.FrameLastActiveSceneView();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("<color=#00ff88>[Kenney Road City Builder]</color> 100% Seamless Ground & Glitch-Free Walking built!");
        }
    }
}
