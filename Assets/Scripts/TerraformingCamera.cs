using UnityEngine;

public class TerraformingCamera : MonoBehaviour
{
	Vector3 _hitPoint;
	Camera _cam;

	public float _brushSize = 2f;

	private void Awake()
  {
		_cam = GetComponent<Camera>();
	}

	private void LateUpdate()
  {
    if (Input.GetKey(KeyCode.LeftShift) == true)
    {
      if (Input.GetMouseButtonDown(0))
      {
        Terraform(true);
      }
      else if (Input.GetMouseButtonDown(1))
      {
        Terraform(false);
      }
    }
    else if (Input.GetKey(KeyCode.LeftShift) == false)
    {
      if (Input.GetMouseButton(0))
      {
        Terraform(true);
      }
      else if (Input.GetMouseButton(1))
      {
        Terraform(false);
      }
    }
	}

	private void Terraform(bool add)
  {
		RaycastHit hit;

		if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out hit, 1000))
    {
			Chunk hitChunk = hit.collider.gameObject.GetComponent<Chunk>();

			_hitPoint = hit.point;

			hitChunk.EditWeights(_hitPoint, _brushSize, add);

      EditNeighboringChunks(hitChunk, _hitPoint, _brushSize, add);
		}
	}

  private void EditNeighboringChunks(Chunk hitChunk, Vector3 hitPoint, float brushSize, bool add)
  {
    float chunkSize = GridMetrics.ChunkScale;

    // Calculate the chunk coordinates of the hit point
    Vector3 hitChunkPosition = hitChunk.transform.position;

    // Calculate bounds of the brush
    Vector3 brushMin = hitPoint - Vector3.one * brushSize;
    Vector3 brushMax = hitPoint + Vector3.one * brushSize;

    // Check all neighboring chunks
    for (int xOffset = -1; xOffset <= 1; xOffset++)
    {
      for (int yOffset = -1; yOffset <= 1; yOffset++)
      {
        for (int zOffset = -1; zOffset <= 1; zOffset++)
        {
          // Calculate the position of the neighboring chunk

          Vector3 neighborChunkPosition = new Vector3(
            hitChunkPosition.x + xOffset * chunkSize,
            hitChunkPosition.y + yOffset * chunkSize,
            hitChunkPosition.z + zOffset * chunkSize);

          // Check if the brush affects this neighboring chunk
          if (IsBrushAffectingChunk(neighborChunkPosition, chunkSize, brushMin, brushMax))
          {
            // Get the neighboring chunk (assume you have a ChunkManager or similar system)
            Chunk neighborChunk = ChunkManager.GetChunkAtPosition(neighborChunkPosition);
            if (neighborChunk == null)
            {
              ChunkManager.CreateChunkFromPlayer(
                neighborChunkPosition,
                ChunkManager.GetNoiseCoordFromWorldCoord(neighborChunkPosition),
                hitPoint,
                brushSize,
                add);
            }
            if (neighborChunk != null && neighborChunk.name != hitChunk.name)
            {
              // Apply the terraform changes to this neighboring chunk
              neighborChunk.EditWeights(hitPoint, _brushSize, add);
            }
          }
        }
      }
    }
  }

  private bool IsBrushAffectingChunk(Vector3 chunkPosition, float chunkSize, Vector3 brushMin, Vector3 brushMax)
  {
    // Calculate the bounding box of the chunk
    Vector3 chunkMin = chunkPosition;
    Vector3 chunkMax = chunkPosition + new Vector3(chunkSize, chunkSize, chunkSize);

    // Check if the brush overlaps with the chunk
    return !(brushMin.x > chunkMax.x || brushMax.x < chunkMin.x ||
      brushMin.y > chunkMax.y || brushMax.y < chunkMin.y ||
      brushMin.z > chunkMax.z || brushMax.z < chunkMin.z);
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(_hitPoint, _brushSize);
  }
}
