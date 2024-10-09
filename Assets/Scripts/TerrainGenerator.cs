using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
  /// <summary>
  ///
  /// </summary>
  /// <param name="chunkPos">The chunk coord to generate at using noise generator.</param>
  /// <returns></returns>
  public static GameObject CreateChunkFromNoise(Vector3 chunkPos, Vector3 noisePos, int LOD)
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
    chunk.MarchingShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
    chunk.LOD = LOD;
    chunk._noisePos = noisePos;
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
    bool add,
    int LOD)
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
    chunk.MarchingShader = Resources.Load<ComputeShader>("Compute/MarchingCubesCompute");
    chunk.LOD = LOD;
    chunk._noisePos = noisePos;
    chunk.CreateFromPlayer(worldHitPosition, brushSize, add);

    return newChunk;
  }
}
