using UnityEngine;

public class YSort : MonoBehaviour
{
    [SerializeField] private int sortingOrderBase = 50000;
    [SerializeField] private int offset = 0;

    private SpriteRenderer spriteRenderer;
    // [SerializeField] private SpriteRenderer graphicsRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        spriteRenderer.sortingOrder =
            sortingOrderBase - Mathf.FloorToInt(transform.position.y * 1000f) + offset;

    }
}