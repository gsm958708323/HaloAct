using UnityEngine;

public class DrawColliders : MonoBehaviour
{
    private Collider colliderComponent;

    void OnDrawGizmos()
    {
        colliderComponent = GetComponent<Collider>();

        if (colliderComponent != null)
        {
            Gizmos.color = Color.yellow;

            if (colliderComponent is BoxCollider)
            {
                DrawBoxCollider((BoxCollider)colliderComponent);
            }
            else if (colliderComponent is SphereCollider)
            {
                DrawSphereCollider((SphereCollider)colliderComponent);
            }
            else if (colliderComponent is CapsuleCollider)
            {
                DrawCapsuleCollider((CapsuleCollider)colliderComponent);
            }
            else if (colliderComponent is MeshCollider)
            {
                DrawMeshCollider((MeshCollider)colliderComponent);
            }
        }
    }

    void DrawBoxCollider(BoxCollider collider)
    {
        Gizmos.DrawWireCube(transform.position + collider.center, collider.size);
    }

    void DrawSphereCollider(SphereCollider collider)
    {
        Gizmos.DrawWireSphere(transform.position + collider.center, collider.radius);
    }

    void DrawCapsuleCollider(CapsuleCollider collider)
    {
        Vector3 center = transform.position + collider.center;

        if (collider.direction == 0) // X-axis
        {
            float radius = collider.radius;
            float height = collider.height - radius * 2.0f;
            Vector3 top = center + Vector3.right * height * 0.5f;
            Vector3 bottom = center - Vector3.right * height * 0.5f;
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawLine(top + Vector3.up * radius, bottom + Vector3.up * radius);
            Gizmos.DrawLine(top - Vector3.up * radius, bottom - Vector3.up * radius);
        }
        else if (collider.direction == 1) // Y-axis
        {
            float radius = collider.radius;
            float height = collider.height - radius * 2.0f;
            Vector3 top = center + Vector3.up * height * 0.5f;
            Vector3 bottom = center - Vector3.up * height * 0.5f;
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
            Gizmos.DrawLine(top - Vector3.right * radius, bottom - Vector3.right * radius);
        }
        else if (collider.direction == 2) // Z-axis
        {
            float radius = collider.radius;
            float height = collider.height - radius * 2.0f;
            Vector3 top = center + Vector3.forward * height * 0.5f;
            Vector3 bottom = center - Vector3.forward * height * 0.5f;
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawLine(top + Vector3.up * radius, bottom + Vector3.up * radius);
            Gizmos.DrawLine(top - Vector3.up * radius, bottom - Vector3.up * radius);
        }
    }

    void DrawMeshCollider(MeshCollider collider)
    {
        if (collider.sharedMesh != null)
        {
            Gizmos.DrawWireMesh(collider.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
        }
    }
}