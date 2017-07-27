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
        private Vector3 _worldSize;

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
            MoveCameras();
            StartSimulation();
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

            _worldSize = data.size;
        }

        private void CreateWalls()
        {
            var wallPrefab = Resources.Load(Naming.Resources.WallPrefab) as GameObject;

            var a = GameObject.Instantiate(wallPrefab);

            a.transform.position = new Vector3(_worldSize.x / 2, .5f, 0);
            a.transform.localScale = new Vector3(_worldSize.x, 1, .5f);

            var b = GameObject.Instantiate(wallPrefab);

            b.transform.position = new Vector3(_worldSize.x / 2, .5f, _worldSize.z);
            b.transform.localScale = new Vector3(_worldSize.x, 1, .5f);

            var c = GameObject.Instantiate(wallPrefab);

            c.transform.position = new Vector3(_worldSize.x, .5f, _worldSize.z / 2);
            c.transform.localScale = new Vector3(.5f, 1, _worldSize.z);

            var d = GameObject.Instantiate(wallPrefab);

            d.transform.position = new Vector3(0, .5f, _worldSize.z / 2);
            d.transform.localScale = new Vector3(.5f, 1, _worldSize.z);
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

        private void MoveCameras()
        {
            var freeCamera = GameObject.FindGameObjectWithTag("FreeCamera").GetComponent<Camera>();
            var mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            MoveCamera(mainCamera);
            MoveCamera(freeCamera);
        }

        private void MoveCamera(Camera camera)
        {
            camera.transform.position = new Vector3(0, (_worldSize.x + _worldSize.z) / 8, 0);
            camera.transform.LookAt(new Vector3(_worldSize.x / 4.5f, 0, _worldSize.z / 4.5f));

            try
            {
                var freeCamera = camera.GetComponent<FreeCamera>();
                freeCamera.ResetView();
            }
            catch { }
            
        }

        private void MoveLight()
        {
            var light = GameObject.Find("Directional Light");

            light.transform.position = new Vector3(_worldSize.x / 2, _worldSize.x + _worldSize.z, _worldSize.z / 2);
            light.transform.LookAt(new Vector3(_worldSize.x / 2, 0, _worldSize.z / 2));
        }

        private void StartSimulation()
        {
            var gameObject = new GameObject("SimulationManager");

            gameObject.AddComponent<SimulationManager>().Begin(_settings);
        }
    }
}
