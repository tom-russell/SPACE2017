using Assets.Scripts.Config;
using Assets.Scripts.Extensions;
using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Arenas
{
    public class ArenaLoader : MonoBehaviour
    {
        public static bool Loading { get; private set; }

        private bool _instantiated;
        private SimulationSettings _settings;
        private Arena _arena;
        public Vector3 worldSize { get; private set; }

        public ArenaLoader()
        {
            Loading = true;
        }

        public void Load(SimulationSettings settings)
        {
            _settings = settings;

#if !UNITY_WEBGL
            using (var sr = new StreamReader(_settings.ArenaFilename))
            {
                var xml = new XmlSerializer(typeof(Arena));

                _arena = xml.Deserialize(sr) as Arena;
            }
#else
            _arena = new Arena
            {
                Depth = 50,
                Width = 50,
                StartingNest = new Nest
                {
                    Width = 5,
                    Depth = 7.5f,
                    PositionX = 40,
                    PositionZ = 25,
                    Quality = 0.25f
                },
                NewNests = new System.Collections.Generic.List<Nest>
                {
                    new Nest
                    {
                        Width =5,
                        Depth = 7.5f,
                        PositionX = 10,
                        PositionZ = 10,
                        Quality = 0.3f
                    },
                    new Nest
                    {
                        Width =5,
                        Depth = 7.5f,
                        PositionX = 10,
                        PositionZ = 40,
                        Quality = 0.6f
                    }
                }
            };
#endif
            SceneManager.LoadScene("FromFile");
        }

        void Update()
        {
            if (!_instantiated)
            {
                _instantiated = true;
                InstantiateArena();
            }
        }

        private void InstantiateArena()
        {
            Loading = true;
            CreateTerrain();
            CreateWalls();
            CreateNests();
            StartSimulation();
            GameObject.Find("CameraOptions").GetComponent<CameraOptions>().SetUpCamera();
            Loading = false;
        }

        private void CreateTerrain()
        {
            var obj = new GameObject("TerrainObj");

            var data = new TerrainData();

            data.size = new Vector3(_arena.Width / 16, 10, _arena.Depth / 16);
            data.heightmapResolution = 512;
            data.baseMapResolution = 1024;
            data.SetDetailResolution(1024, 16);

            var grass = Resources.Load("Grass (Hill)") as Texture2D;

            data.splatPrototypes = new SplatPrototype[]
            {
                new SplatPrototype
                {
                    texture = grass
                }
            };

            var collider = obj.AddComponent<TerrainCollider>();
            var terrain = obj.AddComponent<Terrain>();

            collider.terrainData = data;
            terrain.terrainData = data;

            worldSize = data.size;
        }

        private void CreateWalls()
        {
            var wallPrefab = Resources.Load(Naming.Resources.WallPrefab) as GameObject;

            var a = GameObject.Instantiate(wallPrefab);

            a.transform.position = new Vector3(worldSize.x / 2, .5f, 0);
            a.transform.localScale = new Vector3(worldSize.x, 1, .5f);

            var b = GameObject.Instantiate(wallPrefab);

            b.transform.position = new Vector3(worldSize.x / 2, .5f, worldSize.z);
            b.transform.localScale = new Vector3(worldSize.x, 1, .5f);

            var c = GameObject.Instantiate(wallPrefab);

            c.transform.position = new Vector3(worldSize.x, .5f, worldSize.z / 2);
            c.transform.localScale = new Vector3(.5f, 1, worldSize.z);

            var d = GameObject.Instantiate(wallPrefab);

            d.transform.position = new Vector3(0, .5f, worldSize.z / 2);
            d.transform.localScale = new Vector3(.5f, 1, worldSize.z);
        }

        private void CreateNests()
        {
            if (_arena.StartingNest != null)
            {
                var oldNestPrefab = Resources.Load(Naming.Resources.OldNestPrefab) as GameObject;
                CreateNest(oldNestPrefab, _arena.StartingNest, Naming.World.InitialNest);
            }
            if (_arena.NewNests != null)
            {
                var newNestPrefab = Resources.Load(Naming.Resources.NewNestPrefab) as GameObject;
                var i = 0;
                foreach (var nest in _arena.NewNests)
                    CreateNest(newNestPrefab, nest, Naming.World.NewNests + (i++));
            }
        }

        private void CreateNest(GameObject prefab, Nest nest, string name)
        {
            var nestGO = GameObject.Instantiate(prefab);

            nestGO.name = name;
            nestGO.transform.position = new Vector3(nest.PositionX, 0, nest.PositionZ);
            nestGO.transform.localScale = new Vector3(nest.Width, .1f, nest.Depth);

            nestGO.NestManager().quality = nest.Quality;
        }

        private void MoveLight()
        {
            var light = GameObject.Find("Directional Light");

            light.transform.position = new Vector3(worldSize.x / 2, worldSize.x + worldSize.z, worldSize.z / 2);
            light.transform.LookAt(new Vector3(worldSize.x / 2, 0, worldSize.z / 2));
        }

        private void StartSimulation()
        {
            var gameObject = new GameObject("SimulationManager");

            gameObject.AddComponent<SimulationManager>().Begin(_settings);
        }
    }
}
