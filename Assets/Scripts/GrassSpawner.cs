using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public Mesh grassMesh;
    public Material grassMaterial;
    public int spawnCount = 100;
    public float areaSize = 10f;

    [Header("Collision Settings")]
    public float avoidanceRadius = 0.5f; // Minimum distance from obstacles
    public LayerMask obstaclesLayer;     // Select the layers to avoid in the Inspector

    private List<Renderer> grassRenderers = new List<Renderer>();
    private List<Transform> grassTransforms = new List<Transform>();
    private Camera mainCam;
    private Plane[] frustumPlanes;

    void Start()
    {
        mainCam = Camera.main;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = transform.position + new Vector3(
                Random.Range(-areaSize, areaSize), 
                0, 
                Random.Range(-areaSize, areaSize)
            );

            // Only spawn if the area is clear of objects on the Obstacles Layer
            if (!Physics.CheckSphere(pos, avoidanceRadius, obstaclesLayer))
            {
                GameObject blade = new GameObject("GrassBlade_" + i);
                blade.transform.position = pos;

                MeshFilter mf = blade.AddComponent<MeshFilter>();
                mf.mesh = grassMesh;

                MeshRenderer mr = blade.AddComponent<MeshRenderer>();
                mr.material = grassMaterial;

                grassTransforms.Add(blade.transform);
                grassRenderers.Add(mr);
            }
        }
    }

    void Update()
    {
        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCam);

        for (int i = 0; i < grassTransforms.Count; i++)
        {
            // Use the renderer's bounds to check if it's within the camera view
            bool isInView = GeometryUtility.TestPlanesAABB(frustumPlanes, grassRenderers[i].bounds);

            grassRenderers[i].enabled = isInView;

            if (isInView)
            {
                grassTransforms[i].LookAt(grassTransforms[i].position + mainCam.transform.forward);
            }
        }
    }
}