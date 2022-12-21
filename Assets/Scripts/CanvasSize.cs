using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Canvas))]
public class CanvasSize : MonoBehaviour
{
    public static Vector2 _CanvasSize;
    private void Start()
    {
        _CanvasSize = GetComponent<RectTransform>().sizeDelta;
    }
}
