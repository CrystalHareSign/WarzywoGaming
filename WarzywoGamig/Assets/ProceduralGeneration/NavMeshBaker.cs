using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBaker : MonoBehaviour
{
    public NavMeshSurface surface;

    public void BakeNavMesh()
    {
        surface.BuildNavMesh();
    }
}