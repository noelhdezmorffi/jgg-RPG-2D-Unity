using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]

public class StaticYSort : MonoBehaviour
{
    [SerializeField] private int sortingOrderBase = 50000;
    [SerializeField] private int offset = 0;

    private void Start()
    {
        GetComponent<SpriteRenderer>().sortingOrder =
            sortingOrderBase - Mathf.FloorToInt(transform.position.y * 1000f + offset);
    }
}