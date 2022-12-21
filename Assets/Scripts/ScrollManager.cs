using System.Collections;
using UnityEngine;

public class ScrollManager : MonoBehaviour
{
    [Space(5), SerializeField, Range(0.1f, 2f)]
    private float scrollSpeed = 1f;
    [SerializeField]
    private AnimationCurve scrollCurve;

    public bool PerformingInteraction { get => performingScroll; private set => performingScroll = value; }
    private bool performingScroll = false;

    [Space(10), SerializeField]
    private Swipable centerSwipable;
    [SerializeField]
    private Swipable leftSwipable;
    [SerializeField]
    private Swipable rightSwipable;
    [SerializeField]
    private Swipable upSwipable;
    [SerializeField]
    private Swipable downSwipable;

    private WaitForEndOfFrame frame;

    // Center positions of screens on all sides.
    private Vector2 leftPos;
    private Vector2 rightPos;
    private Vector2 upPos;
    private Vector2 downPos;

    private void Start()
    {
        frame = new();
        centerSwipable.IsCurrentSwipable = true;

        leftPos = leftSwipable.Rect.anchoredPosition;
        rightPos = rightSwipable.Rect.anchoredPosition;
        upPos = upSwipable.Rect.anchoredPosition;
        downPos = downSwipable.Rect.anchoredPosition;
    }

    private void OnEnable()
    {
        InteractionDetection.OnFullSwipe += OnSwipe;
    }
    private void OnDisable()
    {
        InteractionDetection.OnFullSwipe -= OnSwipe;
    }

    private void OnSwipe(Direction direction)
    {
        if (!PerformingInteraction)
            _ = StartCoroutine(SwipeCR());

        IEnumerator SwipeCR()
        {
            switch (direction)
            {
                case Direction.Left:

                    yield return CenterHorizontal(leftPos);

                    // All the 'swipables' move <left<.
                    centerSwipable.IsCurrentSwipable = false;
                    (leftSwipable, centerSwipable, rightSwipable) = (centerSwipable, rightSwipable, leftSwipable);
                    centerSwipable.IsCurrentSwipable = true;

                    SetHorizontalPositions(Vector3.zero);

                    break;


                case Direction.Right:

                    yield return CenterHorizontal(rightPos);

                    // All the 'swipables' move >right>.
                    centerSwipable.IsCurrentSwipable = false;
                    (leftSwipable, centerSwipable, rightSwipable) = (rightSwipable, leftSwipable, centerSwipable);
                    centerSwipable.IsCurrentSwipable = true;

                    SetHorizontalPositions(Vector3.zero);

                    break;


                case Direction.Up:

                    yield return CenterVertical(upPos);

                    // All the 'swipables' move ^up^.
                    centerSwipable.IsCurrentSwipable = false;
                    (downSwipable, centerSwipable, upSwipable) = (upSwipable, downSwipable, centerSwipable);
                    centerSwipable.IsCurrentSwipable = true;

                    SetVerticalPositions(Vector3.zero);

                    break;


                case Direction.Down:

                    yield return CenterVertical(downPos);

                    // All the 'swipables' move ^up^.
                    centerSwipable.IsCurrentSwipable = false;
                    (downSwipable, centerSwipable, upSwipable) = (centerSwipable, upSwipable, downSwipable);
                    centerSwipable.IsCurrentSwipable = true;

                    SetVerticalPositions(Vector3.zero);

                    break;

            }
        }
    }

    private void SetHorizontalPositions(Vector3 centerSwipablePosition)
    {
        centerSwipable.Rect.anchoredPosition = centerSwipablePosition;
        leftSwipable.Rect.anchoredPosition = centerSwipablePosition + Vector3.left * CanvasSize._CanvasSize.x;
        rightSwipable.Rect.anchoredPosition = centerSwipablePosition + Vector3.right * CanvasSize._CanvasSize.x;
    }
    private void SetVerticalPositions(Vector3 centerSwipablePosition)
    {
        centerSwipable.Rect.anchoredPosition = centerSwipablePosition;
        downSwipable.Rect.anchoredPosition = centerSwipablePosition + Vector3.down * CanvasSize._CanvasSize.y;
        upSwipable.Rect.anchoredPosition = centerSwipablePosition + Vector3.up * CanvasSize._CanvasSize.y;
    }
    private void SetAllPositions(Vector3 centerSwipablePosition)
    {
        centerSwipable.Rect.anchoredPosition = centerSwipablePosition;
        leftSwipable.Rect.anchoredPosition = centerSwipablePosition + Vector3.left * CanvasSize._CanvasSize.x;
        rightSwipable.Rect.anchoredPosition = centerSwipablePosition + Vector3.right * CanvasSize._CanvasSize.x;
        downSwipable.Rect.anchoredPosition = centerSwipablePosition + Vector3.down * CanvasSize._CanvasSize.y;
        upSwipable.Rect.anchoredPosition = centerSwipablePosition + Vector3.up * CanvasSize._CanvasSize.y;
    }

    #region Position screens Enums
    private IEnumerator CenterHorizontal(Vector3 targetDestination)
    {
        PerformingInteraction = true;

        Vector2 startPos = centerSwipable.Rect.anchoredPosition;
        Vector2 targetPos = targetDestination;
        float startTime = Time.time;
        float progress = 0f;

        while (progress != 1f)
        {
            float timeSinceStarted = Time.time - startTime;
            progress = scrollCurve.Evaluate(timeSinceStarted / scrollSpeed);

            Vector2 newPos = Vector2.Lerp(startPos, targetPos, progress);
            SetHorizontalPositions(newPos);

            yield return frame;
        }

        PerformingInteraction = false;
    }
    private IEnumerator CenterVertical(Vector3 targetDestination)
    {
        PerformingInteraction = true;

        Vector2 startPos = centerSwipable.Rect.anchoredPosition;
        Vector2 targetPos = targetDestination;
        float startTime = Time.time;
        float progress = 0f;

        while (progress != 1f)
        {
            float timeSinceStarted = Time.time - startTime;
            progress = scrollCurve.Evaluate(timeSinceStarted / scrollSpeed);

            Vector2 newPos = Vector2.Lerp(startPos, targetPos, progress);
            SetVerticalPositions(newPos);

            yield return frame;
        }

        PerformingInteraction = false;
    }
    private IEnumerator CenterAll(Vector3 targetDestination)
    {
        PerformingInteraction = true;

        Vector2 startPos = centerSwipable.Rect.anchoredPosition;
        Vector2 targetPos = targetDestination;
        float startTime = Time.time;
        float progress = 0f;

        while (progress != 1f)
        {
            float timeSinceStarted = Time.time - startTime;
            progress = scrollCurve.Evaluate(timeSinceStarted / scrollSpeed);

            Vector2 newPos = Vector2.Lerp(startPos, targetPos, progress);
            SetAllPositions(newPos);     
            
            yield return frame;
        }

        PerformingInteraction = false;
    }
    #endregion
}

