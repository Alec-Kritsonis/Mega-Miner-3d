using UnityEngine;

public class NoiseGenerator
{
  private static NoiseGenerator _instance;

  ComputeBuffer _weightsBuffer;
  public ComputeShader _noiseShader;

  [SerializeField] float noiseScale = 1f;
  [SerializeField] float amplitude = 60f;
  [SerializeField] float frequency = 0.005f;
  [SerializeField] int octaves = 12;
  [SerializeField, Range(0f, 1f)] float groundPercent = 1f;

  public static NoiseGenerator Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = new NoiseGenerator();
        _instance._noiseShader = Resources.Load<ComputeShader>("Compute/NoiseCompute");
      }

      return _instance;
    }
  }

  private NoiseGenerator() {}

  public float[] GetNoise(int lod, Vector3 worldPos)
  {
    CreateBuffers(lod);
    float[] noiseValues =
      new float[GridMetrics.PointsPerChunk(lod) * GridMetrics.PointsPerChunk(lod) * GridMetrics.PointsPerChunk(lod)];

    _noiseShader.SetBuffer(0, "_Weights", _weightsBuffer);

    _noiseShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk(lod));
    _noiseShader.SetFloat("_NoiseScale", noiseScale);
    _noiseShader.SetFloat("_Amplitude", amplitude);
    _noiseShader.SetFloat("_Frequency", frequency);
    _noiseShader.SetInt("_Octaves", octaves);
    _noiseShader.SetFloat("_GroundPercent", groundPercent);
    _noiseShader.SetInt("_Scale", GridMetrics.NoiseScale);
    _noiseShader.SetInt("_GroundLevel", GridMetrics.GroundLevel);
    _noiseShader.SetVector("_ChunkWorldPos", worldPos);

    _noiseShader.Dispatch(
      0,
      GridMetrics.ThreadGroups(lod),
      GridMetrics.ThreadGroups(lod),
      GridMetrics.ThreadGroups(lod));

    _weightsBuffer.GetData(noiseValues);

    ReleaseBuffers();

    return noiseValues;
  }

  /// <summary>
  /// Truncates any noise not involved in terrain generation.
  /// </summary>
  /// <param name="weights">Piecewised weights.</param>
  public void CleanNoise(ref float[] weights)
  {
    for (int i = 0; i < weights.Length; i++)
    {
      if (weights[i] < 0.0f)
      {
        weights[i] = 0.0f;
      }
      if (weights[i] > 1.0f)
      {
        weights[i] = 1.0f;
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
