using UnityEngine;

public class DynamicCulling : MonoBehaviour
{
    private Renderer[] renderers;
    private Camera mainCamera;
    public float cullingDistance = 50f; // Maksymalna odleg³oœæ renderowania
    public float visibilityMargin = 0.1f; // Margines widocznoœci
    public bool showGizmo = true; // Pokazywanie zasiêgu w edytorze

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>(); // Pobieramy WSZYSTKIE renderery w obiekcie i jego childach
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (renderers.Length == 0 || mainCamera == null) return;

        bool isVisible = IsVisibleWithMargin();
        bool isCloseEnough = Vector3.Distance(transform.position, mainCamera.transform.position) < cullingDistance;

        // W³¹czanie / wy³¹czanie renderowania dla WSZYSTKICH rendererów w childach
        foreach (var renderer in renderers)
        {
            renderer.enabled = isVisible && isCloseEnough;
        }
    }

    bool IsVisibleWithMargin()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        // Powiêkszamy frustum o margines
        for (int i = 0; i < planes.Length; i++)
        {
            planes[i].distance += visibilityMargin;
        }

        // Sprawdzamy widocznoœæ ka¿dego renderera w childach
        foreach (var renderer in renderers)
        {
            if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
                return true; // Jeœli którykolwiek renderer jest widoczny, obiekt ma byæ widoczny
        }

        return false;
    }

    void OnDrawGizmos()
    {
        if (showGizmo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, cullingDistance);
        }
    }
}
