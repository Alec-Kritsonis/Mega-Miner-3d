using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
	public int xSize = 200;
	public int zSize = 200;
  public int inverseResolution = 5;
  public float depth = 40f;
  public float scale = 5f;

  private Mesh mesh;
	private int[] triangles;
	private Vector3[] vertices;

	void Start()
	{
    triangles = new int[xSize * zSize * 6];
    vertices = new Vector3[(xSize + 1) * (zSize + 1)];

		mesh = new Mesh();
    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		GetComponent<MeshFilter>().mesh = mesh;

		CreateShape();
		UpdateMesh();

    GetComponent<MeshCollider>().sharedMesh = mesh;
	}

  ///////////////////////////////

	private void CreateShape()
	{
		for (int i = 0, z = 0; z <= zSize; z++)
		{
			for (int x = 0; x <= xSize; x++)
			{
        float xCoord = (float)x / xSize * scale + 100;
        float zCoord = (float)z / zSize * scale + 100;
				float y = Mathf.PerlinNoise(xCoord, zCoord) * depth;
				vertices[i] = new Vector3(x * inverseResolution, y, z * inverseResolution);
				i++;
			}
		}

		int vert = 0;
		int tris = 0;

		for (int z = 0; z < zSize; z++)
		{
			for (int x = 0; x < xSize; x++)
			{
				triangles[tris + 0] = vert + 0;
				triangles[tris + 1] = vert + xSize + 1;
				triangles[tris + 2] = vert + 1;
				triangles[tris + 3] = vert + 1;
				triangles[tris + 4] = vert + xSize + 1;
				triangles[tris + 5] = vert + xSize + 2;

				vert++;
				tris += 6;
			}
			vert++;
		}
	}

	private void UpdateMesh()
	{
		mesh.Clear();

		mesh.vertices = vertices;
		mesh.triangles = triangles;

		mesh.RecalculateNormals();
	}

  // private void OnDrawGizmos()
  // {
  //   if (vertices == null)
  //     return;

  //   for (int i = 0; i < vertices.Length; i++)
  //   {
  //     Gizmos.DrawSphere(vertices[i], 0.1f);
  //   }
  // }
}