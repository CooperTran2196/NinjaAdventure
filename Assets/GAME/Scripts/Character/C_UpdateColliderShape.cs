using UnityEngine;

/// <summary>
/// Auto-updates PolygonCollider2D to match sprite's Custom Physics Shape each frame.
/// Attach to  GameObject with SpriteRenderer + PolygonCollider2D.
/// Required for animated sprites where sprite position/shape changes per frame.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class C_UpdateColliderShape : MonoBehaviour
{
    SpriteRenderer    spriteRenderer;
    PolygonCollider2D polyCollider;
    Sprite            lastSprite;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polyCollider   = GetComponent<PolygonCollider2D>();
    }

    void LateUpdate()
    {
        // Only update if sprite changed (optimization)
        if (spriteRenderer.sprite != lastSprite)
        {
            lastSprite = spriteRenderer.sprite;
            UpdateColliderShape();
        }
    }

    void UpdateColliderShape()
    {
        if (!lastSprite) return;

        // Try to use Custom Physics Shape first
        int shapeCount = lastSprite.GetPhysicsShapeCount();
        
        if (shapeCount > 0)
        {
            // Use Custom Physics Shape (defined in Sprite Editor)
            polyCollider.pathCount = shapeCount;
            
            for (int i = 0; i < shapeCount; i++)
            {
                var path = new System.Collections.Generic.List<Vector2>();
                lastSprite.GetPhysicsShape(i, path);
                polyCollider.SetPath(i, path);
            }
        }
        else
        {
            // Fallback: Generate from sprite outline automatically
            Destroy(polyCollider);
            polyCollider = gameObject.AddComponent<PolygonCollider2D>();
        }
    }

    // Update collider on enable (when first added or re-enabled)
    void OnEnable()
    {
        if (spriteRenderer && polyCollider)
        {
            lastSprite = spriteRenderer.sprite;
            UpdateColliderShape();
        }
    }
}
