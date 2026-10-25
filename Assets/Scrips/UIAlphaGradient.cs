
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UIAlphaGradient : MonoBehaviour
{
    [SerializeField] private int textureWidth = 256;
    [SerializeField] private int textureHeight = 256;
    [SerializeField] private bool reverseDirection = false;

    private void Awake()
    {
        Texture2D texture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGBA32,
            false
        );

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < textureWidth; x++)
        {
            float alpha = (float)x / (textureWidth - 1);

            if (reverseDirection)
                alpha = 1f - alpha;

            for (int y = 0; y < textureHeight; y++)
            {
                texture.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }

        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0.5f),
            100f
        );

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.white;
    }
}
