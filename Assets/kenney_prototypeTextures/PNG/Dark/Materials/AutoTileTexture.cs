using UnityEngine;

public class AutoTileTexture : MonoBehaviour
{
    public float tileSize = 1f;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        Vector3 scale = transform.localScale;

        rend.material.mainTextureScale = new Vector2(
            scale.x / tileSize,
            scale.z / tileSize
        );
    }
}