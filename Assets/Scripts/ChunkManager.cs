using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
  [SerializeField]
  private static NoiseGenerator noiseGenerator;

  private const int _WorldSize = 20;
  private const int _LOD = 4;
  private Dictionary<Vector3Int, GameObject> chunks = new Dictionary<Vector3Int, GameObject>();

  private int count;
  private Color[] colors = { Color.white, Color.black, Color.cyan, Color.blue, Color.magenta, Color.red, Color.yellow, Color.green };
  private GameObject[] visuals = new GameObject[_WorldSize * _WorldSize];

  public void Remake()
  {
    DestroyAllChunks();
    CreateTerrain(_WorldSize);
  }

  void Start()
  {
    count = 0;
    GameObject noiseObj = new GameObject("NoiseGenerator");
    noiseGenerator = noiseObj.AddComponent<NoiseGenerator>();
    noiseGenerator.NoiseShader = Resources.Load<ComputeShader>("Compute/NoiseCompute");

    CreateTerrain(_WorldSize);
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="worldSize">The length * width of the world in chunk coords.</param>
  void CreateTerrain(int worldSize)
  {
    Debug.Log("VALIDATE");
    // float offset = (float)GridMetrics.Scale / (GridMetrics.PointsPerChunk(_LOD) + 1);
    for (int x = 0; x < worldSize; x++)
    {
      for (int z = 0; z < worldSize; z++)
      {
        int[] verticalChunks = FindVerticalChunks(x, z);

        foreach (int y in verticalChunks)
        {
          chunks.Add(
            new Vector3Int(x, y, z),
            CreateChunkFromNoise(
              new Vector3(
                x * GridMetrics.ChunkScale,
                y * GridMetrics.ChunkScale,
                z * GridMetrics.ChunkScale),
              new Vector3(
                x * GridMetrics.NoiseScale,
                y * GridMetrics.NoiseScale,
                z * GridMetrics.NoiseScale)));
            // Can slightly fix chunk borders at lower LOD. I assume noise is fine, main issue has to be from marching algorithm
            // new Vector3(x * offset, 0, z * offset));

          // visuals[x + z] = createVisual(new Vector3(x * GridMetrics.Scale, 0, z * GridMetrics.Scale));
        }

      }
    }
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="chunkPos">The chunk coord to generate at using noise generator.</param>
  /// <returns></returns>
  public static GameObject CreateChunkFromNoise(Vector3 chunkPos, Vector3 noisePos)
  {
    int chunkX = Mathf.FloorToInt(chunkPos.x / GridMetrics.ChunkScale);
    int chunkY = Mathf.FloorToInt(chunkPos.y / GridMetrics.ChunkScale);
    int chunkZ = Mathf.FloorToInt(chunkPos.z / GridMetrics.ChunkScale);

    GameObject newChunk = new GameObject($"Chunk-{chunkX}.{chunkY}.{chunkZ}");
    newChunk.transform.position = chunkPos;

    MeshFilter meshFilter = newChunk.AddComponent<MeshFilter>();
    MeshCollider meshCollider = newChunk.AddComponent<MeshCollider>();
    MeshRenderer meshRenderer = newChunk.AddComponent<MeshRenderer>();
    Chunk chunk = newChunk.AddComponent<Chunk>();

    meshRenderer.material = Resources.Load<Material>("Materials/Ground");
    // meshRenderer.material = Resources.Load<Material>("Materials/Wireframe");

    chunk.MeshFilter = meshFilter;
    chunk.MeshCollider = meshCollider;
    chunk.NoiseGenerator = noiseGenerator;
    chunk.MarchingShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
    chunk.LOD = _LOD;
    chunk._noisePos = noisePos;
    // chunk._ChunkWorldPos = chunkPos - offset;  // For "fixing" lower LOD borders.
    chunk.CreateFromNoise();

    return newChunk;
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="chunkPos">The chunk coord to generate at using just the edit weights.</param>
  /// <returns></returns>
  public static GameObject CreateChunkFromPlayer(
    Vector3 chunkPos,
    Vector3 noisePos,
    Vector3 worldHitPosition,
    float brushSize,
    bool add)
  {
    int chunkX = Mathf.FloorToInt(chunkPos.x / GridMetrics.ChunkScale);
    int chunkY = Mathf.FloorToInt(chunkPos.y / GridMetrics.ChunkScale);
    int chunkZ = Mathf.FloorToInt(chunkPos.z / GridMetrics.ChunkScale);

    GameObject newChunk = new GameObject($"Chunk-{chunkX}.{chunkY}.{chunkZ}");
    newChunk.transform.position = chunkPos;

    MeshFilter meshFilter = newChunk.AddComponent<MeshFilter>();
    MeshCollider meshCollider = newChunk.AddComponent<MeshCollider>();
    MeshRenderer meshRenderer = newChunk.AddComponent<MeshRenderer>();
    Chunk chunk = newChunk.AddComponent<Chunk>();

    meshRenderer.material = Resources.Load<Material>("Materials/Ground");
    // meshRenderer.material = Resources.Load<Material>("Materials/Wireframe");

    chunk.MeshFilter = meshFilter;
    chunk.MeshCollider = meshCollider;
    chunk.NoiseGenerator = noiseGenerator;
    chunk.MarchingShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
    chunk.LOD = _LOD;
    chunk._noisePos = noisePos;
    // chunk._ChunkWorldPos = chunkPos - offset;  // For "fixing" lower LOD borders.
    chunk.CreateFromPlayer(worldHitPosition, brushSize, add);

    return newChunk;
  }

  /// <summary>
  /// Finds if the noise for a chunk has below and above ground values, meaning
  /// it's surface level.
  /// </summary>
  /// <param name="x">Chunk coord x.</param>
  /// <param name="z">Chunk coord z.</param>
  /// <returns>List of vertical chunks at the given x, z chunk coord.</returns>
  int[] FindVerticalChunks(int x, int z)
  {
    List<int> verticalChunks = new List<int>();
    for (int y = 0; y < GridMetrics.VerticalChunks; y++)
    {
      bool negativeNoise = false;
      bool positiveNoise = false;
      float[] noise = noiseGenerator.GetNoise(_LOD, new Vector3(x * GridMetrics.NoiseScale, y * GridMetrics.NoiseScale, z * GridMetrics.NoiseScale));
      for (int i = 0; i < noise.Length; i++)
      {
        // if (x == 8 && z == 4 && y == 1 && i > noise.Length - GridMetrics.PointsPerChunk(_LOD) * GridMetrics.PointsPerChunk(_LOD))
        // {
        //   Debug.Log(noise[i]);
        // }
        if (noise[i] < 0.5)
        {
          negativeNoise = true;
        }
        else if (noise[i] >= 0.5)
        {
          positiveNoise = true;
        }

        if (positiveNoise && negativeNoise)
        {
          verticalChunks.Add(y);

          break;
        }
      }
    }

    return verticalChunks.ToArray();
  }

  GameObject createVisual(Vector3 chunkPos)
  {
    GameObject newVisual = new GameObject("Visual");
    newVisual.transform.position = chunkPos;
    NoiseVisual visual = newVisual.AddComponent<NoiseVisual>();

    visual.NoiseGenerator = noiseGenerator;
    visual.LOD = _LOD;
    visual._ChunkWorldPos = chunkPos;
    visual.color1 = colors[count];
    visual.color2 = colors[count + 1];

    count += 2;
    return newVisual;
  }

  /// <summary>
  /// Translates world position to Chunk object.
  /// </summary>
  /// <param name="worldPos">The world position to find a chunk at.</param>
  /// <returns></returns>
  public static Chunk GetChunkAtPosition(Vector3 worldPos)
  {
    Vector3 chunkPos = GetChunkCoordFromWorldCoord(worldPos);

    return GameObject.Find($"Chunk-{chunkPos.x}.{chunkPos.y}.{chunkPos.z}")?.GetComponent<Chunk>();
  }

  /// <summary>
  /// Translate world coord to chunk coord.
  /// </summary>
  /// <param name="worldPos">The world position to translate.</param>
  /// <returns></returns>
  public static Vector3 GetChunkCoordFromWorldCoord(Vector3 worldPos)
  {
    return new Vector3(
      Mathf.FloorToInt(worldPos.x / GridMetrics.ChunkScale),
      Mathf.FloorToInt(worldPos.y / GridMetrics.ChunkScale),
      Mathf.FloorToInt(worldPos.z / GridMetrics.ChunkScale));
  }

  /// <summary>
  /// Translate world coord to noise coord.
  /// </summary>
  /// <param name="worldPos">The world position to translate.</param>
  /// <returns></returns>
  public static Vector3 GetNoiseCoordFromWorldCoord(Vector3 worldPos)
  {
    Debug.Log($"World: {worldPos} - Chunk: {GetChunkCoordFromWorldCoord(worldPos)} - Noise? {GetChunkCoordFromWorldCoord(worldPos) * GridMetrics.ChunkScale / GridMetrics.NoiseScale}");
    return GetChunkCoordFromWorldCoord(worldPos) * GridMetrics.ChunkScale / GridMetrics.NoiseScale;
  }

  private void DestroyAllChunks()
  {
    foreach (var chunkPair in chunks)
    {
      Chunk chunk = chunkPair.Value.GetComponent<Chunk>();

      // Destroy the chunk GameObject
      Destroy(chunkPair.Value);
    }

    // Clear the dictionary
    chunks.Clear();
  }
}
