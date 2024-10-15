using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkUtils : MonoBehaviour
{
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
