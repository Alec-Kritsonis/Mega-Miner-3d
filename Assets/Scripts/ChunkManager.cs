using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
  private const int _RenderDistance = 10;
  private const int _LOD = 4;
  private static Dictionary<Vector3Int, GameObject> chunks = new Dictionary<Vector3Int, GameObject>();

  private static Vector3Int _playerChunkPos = new Vector3Int(0, 0, 0);

  public static void UpdatePlayerPosition(Vector3 playerPos)
  {

    if (GetChunkCoordFromWorldCoord(playerPos) != _playerChunkPos)
    {
      _playerChunkPos = GetChunkCoordFromWorldCoord(playerPos);
      GenerateChunksAroundPlayer();
      RemoveFarChunks();
    }
  }

  // TODO load player made chunks
  static void GenerateChunksAroundPlayer()
  {
    for (int chunkX = _playerChunkPos.x - _RenderDistance; chunkX < _playerChunkPos.x + _RenderDistance; chunkX++)
    {
      for (int chunkZ = _playerChunkPos.z - _RenderDistance; chunkZ < _playerChunkPos.z + _RenderDistance; chunkZ++)
      {
        int[] verticalChunks = FindVerticalChunks(chunkX, chunkZ);

        foreach (int chunkY in verticalChunks)
        {
          Vector3Int chunkPos = new Vector3Int(chunkX, chunkY, chunkZ);
          if (!chunks.ContainsKey(chunkPos))
          {
            chunks.Add(
              chunkPos,
              TerrainGenerator.CreateChunkFromNoise(
                new Vector3(
                  chunkX * GridMetrics.ChunkScale,
                  chunkY * GridMetrics.ChunkScale,
                  chunkZ * GridMetrics.ChunkScale),
                new Vector3(
                  chunkX * GridMetrics.NoiseScale,
                  chunkY * GridMetrics.NoiseScale,
                  chunkZ * GridMetrics.NoiseScale),
                _LOD));
          }
        }
      }
    }
  }

  public static void CreateChunkFromPlayer(
    Vector3Int chunkPos,
    Vector3 noisePos,
    Vector3 worldHitPosition,
    float brushSize,
    bool add)
  {
    chunks.Add(
      chunkPos,
      TerrainGenerator.CreateChunkFromPlayer(chunkPos, noisePos, worldHitPosition, brushSize, add, _LOD));
  }

  static void RemoveFarChunks()
  {
    List<Vector3Int> chunksToRemove = new List<Vector3Int>();

    foreach (Vector3Int chunkPos in chunks.Keys)
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
        Destroy(chunks[chunkPos]);
        chunks.Remove(chunkPos);
      }
    }
  }

  static bool IsChunkInRenderDistance(Vector3Int chunkPos)
  {
    Vector3 distance = chunkPos - _playerChunkPos;

    return Mathf.Abs(distance.x) <= _RenderDistance &&
      Mathf.Abs(distance.y) <= _RenderDistance &&
      Mathf.Abs(distance.z) <= _RenderDistance;
  }


  /// <summary>
  /// Finds if the noise for a chunk has below and above ground values, meaning
  /// it's surface level.
  /// </summary>
  /// <param name="x">Chunk coord x.</param>
  /// <param name="z">Chunk coord z.</param>
  /// <returns>List of vertical chunks at the given x, z chunk coord.</returns>
  static int[] FindVerticalChunks(int x, int z)
  {
    List<int> verticalChunks = new List<int>();
    for (int y = 0; y < GridMetrics.VerticalChunks; y++)
    {
      bool negativeNoise = false;
      bool positiveNoise = false;
      float[] noise = NoiseGenerator.Instance.GetNoise(
        _LOD,
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

  //////////////////////////////////////////////////////////////////////////////
  /// Util
  //////////////////////////////////////////////////////////////////////////////

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
  public static Vector3Int GetChunkCoordFromWorldCoord(Vector3 worldPos)
  {
    return new Vector3Int(
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
    return GetChunkCoordFromWorldCoord(worldPos) * GridMetrics.ChunkScale / GridMetrics.NoiseScale;
  }
}
