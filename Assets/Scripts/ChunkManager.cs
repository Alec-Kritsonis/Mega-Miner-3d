using Unity.Burst;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.VisualScripting;
using System.Linq;

public class ChunkManager : MonoBehaviour
{
  public static ChunkManager Instance { get; private set; }

  private const int _RenderDistance = 10;
  private const int _LOD = 4;

  private static Dictionary<Vector3Int, GameObject> _loadedChunks = new Dictionary<Vector3Int, GameObject>();
  private static SortedList<float, Vector3Int> _chunkQueue = new SortedList<float, Vector3Int>();
  private static Vector3Int _playerChunkPos = new Vector3Int(-100000, 0, -100000);
  // TODO: Octrees or better spatial data strucutres.
  private static Dictionary<Vector2Int, HashSet<int>> _discoveredChunks = new Dictionary<Vector2Int, HashSet<int>>();

  private NativeArray<Vector3Int> chunkPositionsToGenerate;

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(this.gameObject);
    }
  }

  public void UpdatePlayerPosition(Vector3 playerPos)
  {
    Vector3Int previousChunkPos = ChunkUtils.GetChunkCoordFromWorldCoord(playerPos);
    if (previousChunkPos.x != _playerChunkPos.x && previousChunkPos.z != _playerChunkPos.z)
    {
      _playerChunkPos = ChunkUtils.GetChunkCoordFromWorldCoord(playerPos);
      QueueNearbyChunks();
      RemoveFarChunks();
      GenerateChunksFromQueue();
    }
  }

  // TODO: load player made chunks
  void QueueNearbyChunks()
  {
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();
    for (int chunkX = _playerChunkPos.x - _RenderDistance; chunkX < _playerChunkPos.x + _RenderDistance; chunkX++)
    {
      for (int chunkZ = _playerChunkPos.z - _RenderDistance; chunkZ < _playerChunkPos.z + _RenderDistance; chunkZ++)
      {
        int[] verticalChunkCoords;
        Vector2Int chunkSlice = new Vector2Int(chunkX, chunkZ);

        if (_discoveredChunks.ContainsKey(chunkSlice))
        {
          verticalChunkCoords = _discoveredChunks[chunkSlice].ToArray();
        }
        else
        {
          verticalChunkCoords = FindVerticalChunks(chunkX, chunkZ);
          _discoveredChunks.Add(chunkSlice, new HashSet<int>(verticalChunkCoords));
        }

        foreach (int chunkY in verticalChunkCoords)
        {
          Vector3Int chunkPos = new Vector3Int(chunkX, chunkY, chunkZ);
          float chunkDistance = Vector2.Distance(
            new Vector2(_playerChunkPos.x, _playerChunkPos.z),
            new Vector2(chunkPos.x, chunkPos.z));

          if (!_loadedChunks.ContainsKey(chunkPos) && !_chunkQueue.ContainsKey(chunkDistance))
          {
            _chunkQueue.Add(chunkDistance, chunkPos);
          }
        }
      }
    }
    stopwatch.Stop();
    UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds);
  }

  void GenerateChunksFromQueue()
  {
    while (_chunkQueue.Count > 0)
    {
      Vector3Int chunkPos = _chunkQueue.Values[0];
      _chunkQueue.RemoveAt(0);

      _loadedChunks.Add(
        chunkPos,
        TerrainGenerator.CreateChunkFromNoise(
          new Vector3(
            chunkPos.x * GridMetrics.ChunkScale,
            chunkPos.y * GridMetrics.ChunkScale,
            chunkPos.z * GridMetrics.ChunkScale),
          new Vector3(
            chunkPos.x * GridMetrics.NoiseScale,
            chunkPos.y * GridMetrics.NoiseScale,
            chunkPos.z * GridMetrics.NoiseScale),
          _LOD));
    }
  }

  void RemoveFarChunks()
  {
    List<Vector3Int> chunksToRemove = new List<Vector3Int>();

    foreach (Vector3Int chunkPos in _loadedChunks.Keys)
    {
      if (!IsChunkInRenderDistance(chunkPos))
      {
        chunksToRemove.Add(chunkPos);
      }
    }

    foreach (Vector3Int chunkPos in chunksToRemove)
    {
      if (!IsChunkInRenderDistance(chunkPos))
      {
        Destroy(_loadedChunks[chunkPos]);
        _loadedChunks.Remove(chunkPos);
      }
    }
  }

  bool IsChunkInRenderDistance(Vector3Int chunkPos)
  {
    Vector3 distance = chunkPos - _playerChunkPos;

    return Mathf.Abs(distance.x) <= _RenderDistance &&
      Mathf.Abs(distance.y) <= _RenderDistance &&
      Mathf.Abs(distance.z) <= _RenderDistance;
  }

  public void CreateChunkFromPlayer(
    Vector3Int chunkPos,
    Vector3 noisePos,
    Vector3 worldHitPosition,
    float brushSize,
    bool add)
  {
    _loadedChunks.Add(
      chunkPos,
      TerrainGenerator.CreateChunkFromPlayer(chunkPos, noisePos, worldHitPosition, brushSize, add, _LOD));
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
      float[] noise = NoiseGenerator.Instance.GetNoise(
        0,
        new Vector3(
          x * GridMetrics.NoiseScale,
          y * GridMetrics.NoiseScale,
          z * GridMetrics.NoiseScale));

      for (int i = 0; i < noise.Length; i++)
      {
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
}
