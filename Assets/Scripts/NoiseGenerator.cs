using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
  ComputeBuffer _weightsBuffer;
  public ComputeShader NoiseShader;

  [SerializeField] float noiseScale = 1f;
  [SerializeField] float amplitude = 60f;
  [SerializeField] float frequency = 0.005f;
  [SerializeField] int octaves = 12;
  [SerializeField, Range(0f, 1f)] float groundPercent = 1f;

  void OnValidate()
  {
    ChunkManager c = GameObject.Find("ChunkManager")?.GetComponent<ChunkManager>();
    c.Remake();
  }

  public float[] GetNoise(int lod, Vector3 worldPos)
  {
    CreateBuffers(lod);
    float[] noiseValues =
      new float[GridMetrics.PointsPerChunk(lod) * GridMetrics.PointsPerChunk(lod) * GridMetrics.PointsPerChunk(lod)];

    NoiseShader.SetBuffer(0, "_Weights", _weightsBuffer);

    NoiseShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk(lod));
    NoiseShader.SetFloat("_NoiseScale", noiseScale);
    NoiseShader.SetFloat("_Amplitude", amplitude);
    NoiseShader.SetFloat("_Frequency", frequency);
    NoiseShader.SetInt("_Octaves", octaves);
    NoiseShader.SetFloat("_GroundPercent", groundPercent);
    NoiseShader.SetInt("_Scale", GridMetrics.NoiseScale);
    NoiseShader.SetInt("_GroundLevel", GridMetrics.GroundLevel);
    NoiseShader.SetVector("_ChunkWorldPos", worldPos);

    NoiseShader.Dispatch(
      0,
      GridMetrics.ThreadGroups(lod),
      GridMetrics.ThreadGroups(lod),
      GridMetrics.ThreadGroups(lod));

    _weightsBuffer.GetData(noiseValues);

    ReleaseBuffers();

    return noiseValues;
  }

  public void CleanNoise(ref float[] weights)
  {
    for (int i = 0; i < weights.Length; i++)
    {
      if (weights[i] > GridMetrics.IsoLevel)
      {
        weights[i] = 0.0f;
      }
    }
  }

  void CreateBuffers(int lod)
  {
    _weightsBuffer = new ComputeBuffer(
      GridMetrics.PointsPerChunk(lod) * GridMetrics.PointsPerChunk(lod) * GridMetrics.PointsPerChunk(lod),
      sizeof(float));
  }

  void ReleaseBuffers()
  {
    _weightsBuffer.Release();
  }
}
