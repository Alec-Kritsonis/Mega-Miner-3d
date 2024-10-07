using UnityEngine;

public class NoiseVisual : MonoBehaviour
{
  public NoiseGenerator NoiseGenerator;
  public int LOD;
  public Vector3 _ChunkWorldPos;
  public Color color1;
  public Color color2;

  float[] _weights;

  void Start()
  {
    NoiseGenerator = GameObject.Find("NoiseGenerator").GetComponent<NoiseGenerator>();
    _weights = NoiseGenerator.GetNoise(LOD, _ChunkWorldPos);
  }

  private void OnDrawGizmos()
  {
    if (_weights == null || _weights.Length == 0)
    {
      return;
    }

    int PointsPerChunk = GridMetrics.PointsPerChunk(LOD);
    for (int x = 0; x < PointsPerChunk; x++)
    {
      for (int y = 0; y < PointsPerChunk; y++)
      {
        for (int z = 0; z < PointsPerChunk; z++)
        {
          int index = x + PointsPerChunk * (y + PointsPerChunk * z);
          float noiseValue = _weights[index];
          if (x == 0 && z == 0)
          {
            Debug.Log(y + " : " + noiseValue);
          }
          Gizmos.color = Color.Lerp(color2, color1, noiseValue);
          Gizmos.DrawCube(new Vector3(
            (float)x / (PointsPerChunk - 1) * GridMetrics.ChunkScale + _ChunkWorldPos.x,
            (float)y / (PointsPerChunk - 1) * GridMetrics.ChunkScale + _ChunkWorldPos.y,
            (float)z / (PointsPerChunk - 1) * GridMetrics.ChunkScale + _ChunkWorldPos.z),
          Vector3.one * .2f);
        }
      }
    }
  }
}