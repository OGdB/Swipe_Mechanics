using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Swipable : MonoBehaviour
{
    #region Properties
    public RectTransform Rect { get => rect; private set => rect = value; }
    private RectTransform rect;

    public RectTransform ImageRect { get => imageRect; private set => imageRect = value; }
    private RectTransform imageRect;
    public AspectRatioFitter Fitter { get => fitter; private set => fitter = value; }

    private AspectRatioFitter fitter;

    [SerializeField, Tooltip("The amount of seconds it takes to perform the zooming.")]
    private float zoomSpeed = 1.25f;
    [SerializeField, Tooltip("The curve at which the speed of the zoom is evaluted.")]
    private AnimationCurve zoomCurve;

    [SerializeField]
    private float autoPanSpeed = 2f;
    [SerializeField]
    private float autopanPauseAtEdges = 1.5f;
    Vector3 currentAutoPanDirection = Vector3.left;

    public bool IsCurrentSwipable { get => isCurrentSwipable; set => OnChangeCurrent(value); }
    private bool isCurrentSwipable = false;
    private Coroutine autoPanCR;
    private bool paused = false;

    public bool ZoomedIn { get => zoomedIn; set => Zoomed(value); }
    private bool zoomedIn = false; // The current zoom state.
    private bool zooming = false; // Is there a zooming 'animation' ongoing?

    private WaitForEndOfFrame frame;
    private WaitForSeconds secPause;
    #endregion

    private void Awake()
    {
        Rect = GetComponent<RectTransform>();
        ImageRect = transform.GetChild(0).GetComponent<RectTransform>();
        Fitter = transform.GetChild(0).GetComponent<AspectRatioFitter>();

        ZoomedIn = Fitter.aspectMode == AspectRatioFitter.AspectMode.HeightControlsWidth ? true : false;
        frame = new();
        secPause = new(autopanPauseAtEdges);
    }

    private void OnChangeCurrent(bool value)
    {
        isCurrentSwipable = value;

        if (value)
        {
            InteractionDetection.OnDoubleTap += ZoomImage;
        }
        else
        {
            InteractionDetection.OnDoubleTap -= ZoomImage;
        }
    }

    private void ZoomImage()
    {
        if (zooming) return;

        Fitter.aspectMode = ZoomedIn ? AspectRatioFitter.AspectMode.WidthControlsHeight : AspectRatioFitter.AspectMode.HeightControlsWidth;

        _ = StartCoroutine(Zoom());


        IEnumerator Zoom()
        {
            zooming = true;

            float startTime = Time.time;
            Vector2 startSizeDelta = ImageRect.sizeDelta;
            Vector2 targetSizeDelta = Vector2.zero;
            float progress = 0f;
            print("Start zoom");

            while (progress <= 1f)
            {
                float timeSinceStarted = Time.time - startTime;
                progress = timeSinceStarted / zoomSpeed;

                imageRect.sizeDelta = Vector2.Lerp(startSizeDelta, targetSizeDelta, zoomCurve.Evaluate(progress));

                yield return frame;
            }

            imageRect.sizeDelta = targetSizeDelta;
            print("Zoomed in!");

            ZoomedIn = !ZoomedIn;
            zooming = false;
        }
    }

    private void Zoomed(bool value)
    {
        zoomedIn = value;

        // Set Panning options/abilities
        if (zoomedIn)
        {
            StartCoroutine(AutoPan(secPause));
        }
    }

    private IEnumerator AutoPan(WaitForSeconds startDelay = null)
    {
        yield return startDelay;
        float rectHalf = imageRect.sizeDelta.x / 2;

        while (true)
        {
            while (!paused)
            {
                imageRect.Translate(currentAutoPanDirection * autoPanSpeed);

                if (Mathf.Abs(imageRect.anchoredPosition.x) >= rectHalf)
                {
                    currentAutoPanDirection = currentAutoPanDirection == Vector3.right ? -Vector3.right : Vector3.right;
                    yield return secPause;
                }

                imageRect.localPosition = Vector2.ClampMagnitude(imageRect.localPosition, rectHalf);

                yield return frame;
            }
        }
    }
}
