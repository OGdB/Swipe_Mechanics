using UnityEngine;

public class InteractionDetection : MonoBehaviour
{
    #region Properties
    public static InteractionDetection _Instance;

    [SerializeField, Tooltip("Minimum velocity to be considered a swipe")]
    float minimumSwipeMagnitude = 1;
    [SerializeField, Range(0.5f, 2f), Tooltip("The amount of seconds to hold the image until you are in panning mode.")]
    float secondsUntilPanMode = 1f;
    [SerializeField, Range(0.5f, 2f), Tooltip("The amount of seconds after letting go of the image until out of panning mode.")]
    float secondsToEndPanMode = 0.75f;
    [SerializeField]
    private float minimumSwipeLength = 150f;
    private float longestSwipeLength;

    float startTime = 0;
    float endTime = 0;
    float totalTouchTime = 0; // Amount of seconds touching screen
    float swipeMagnitude;
    Vector2 startPos;
    Vector2 endPos;
    int tapCount = 0;

    public static Direction CurrentDirection { get => currentDirection; private set => currentDirection = value; }
    private static Direction currentDirection;
    public static Axis CurrentAxis { get => currentAxis; private set => currentAxis = value; }
    private static Axis currentAxis;
    public static Vector2 TouchLength { get => touchLength; private set => touchLength = value; }
    private static Vector2 touchLength; // The length in pixels that the user has touched the screen
    public static Vector2 LastSwipeVelocity { get => lastSwipeVelocity; set => lastSwipeVelocity = value; }
    private static Vector2 lastSwipeVelocity = Vector2.zero;
    public static Vector2 TouchDelta { get => delta; private set => delta = value; }
    private static Vector2 delta;
    public static Vector2 LastTouchDelta { get => lastDelta; private set => lastDelta = value; }
    private static Vector2 lastDelta;
    public static bool IsTouchingScreen { get => touchingScreen; private set => touchingScreen = value; }
    private static bool touchingScreen = false;

    private bool panningMode = false;

    #region Events
    public delegate void PanningStartDelegate();
    public static PanningStartDelegate OnPanningStart;

    public delegate void PanningEndDelegate();
    public static PanningStartDelegate OnPanningEnd;

    public delegate void SwipeDelegate(Direction direction);
    public static SwipeDelegate OnFullSwipe;

    public delegate void Sliding(float deltaFloat);
    public static Sliding OnSlidingHorizontal;
    public static Sliding OnSlidingVertical;

    public delegate void StopTouch();
    public static StopTouch OnTouchEnd;

    public delegate void DoubleTap();
    public static StopTouch OnDoubleTap;
    #endregion

    #endregion

    private void Awake()
    {
        if (!_Instance)
        {
            _Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void LateUpdate()
    {
        TouchDelta = default;

        // If the release time is longer than 2 seconds ago and not panning
        if (panningMode && !IsTouchingScreen && Time.time - endTime >= secondsToEndPanMode)
        {
            panningMode = false;
            OnPanningEnd?.Invoke();
        }

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                LastSwipeVelocity = Vector2.zero;

                startPos = touch.position;
                startTime = Time.time;
                IsTouchingScreen = true;

                if (startTime - endTime >= 0.3f && tapCount == 1)
                {
                    tapCount = 0;
                }

                // Disable panmode if there is no interaction for X amount of seconds
                if (startTime - endTime > secondsToEndPanMode)
                    panningMode = false;
            }

            totalTouchTime = Time.time - startTime;
            Vector2 swipeDiff = touch.position - startPos;

            if (swipeDiff.magnitude > longestSwipeLength)
                longestSwipeLength = swipeDiff.magnitude;

            if (touch.phase == TouchPhase.Moved)
            {
                TouchLength = swipeDiff;
                TouchDelta = touch.deltaPosition;
                swipeMagnitude = touch.deltaPosition.magnitude;

                if (swipeDiff.magnitude >= minimumSwipeLength)
                {
                    if (CurrentAxis == Axis.None)
                        CurrentAxis = GetAxis(swipeDiff);

                    Vector2 lengthAbs = touchLength.Abs();
                    if (CurrentAxis == Axis.Horizontal)
                        OnSlidingHorizontal?.Invoke(TouchDelta.x);
                    else
                        OnSlidingVertical?.Invoke(TouchDelta.y);
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                //print("End");
                if (totalTouchTime < 0.3f)
                {
                    tapCount += 1;

                    if (tapCount == 2)
                    {
                        OnDoubleTap?.Invoke();
                        //print("Double Tap!");
                        tapCount = 0;
                    }
                }

                endTime = Time.time;
                endPos = touch.position;
                lastDelta = touch.deltaPosition;

                float swipeDistance = Vector2.Distance(startPos, endPos);
                LastSwipeVelocity = swipeDiff / (endTime - startTime);

                // Swipe conditions:
                // Not panning, swiped fast enough & 'long' enough
                bool swipingConditions = !panningMode && swipeMagnitude > minimumSwipeMagnitude && swipeDistance >= minimumSwipeLength;
                if (swipingConditions)
                {
                    currentDirection = GetSwipeDirection(swipeDiff);
                    OnFullSwipe?.Invoke(currentDirection);
                    //print($"Swipe {currentDirection}!");

                }

                OnTouchEnd?.Invoke();

                ResetFields();
            }

            // Holding down finger on picture for X amount of seconds without moving finger
            PanmodeDetection(longestSwipeLength);
        }



        void PanmodeDetection(float swipeLength)
        {
            if (!panningMode && totalTouchTime > secondsUntilPanMode && swipeLength < minimumSwipeLength)
            {
                panningMode = true;
                OnPanningStart?.Invoke();
                //print("Panning!");
            }
        }
    }

    private Direction GetSwipeDirection(Vector2 swipeLine)
    {
        Direction direction = Direction.None;

        if (Mathf.Abs(swipeLine.y) > Mathf.Abs(swipeLine.x))
        {
            direction = swipeLine.y > 0 ? Direction.Up : Direction.Down;
        }
        else if (Mathf.Abs(swipeLine.x) > Mathf.Abs(swipeLine.y))
        {
            direction = swipeLine.x > 0 ? Direction.Right : Direction.Left;
        }

        return direction;
    }
    private Axis GetAxis(Vector2 swipeLine)
    {
        Axis axis = Axis.None;

        if (Mathf.Abs(swipeLine.y) > Mathf.Abs(swipeLine.x))
        {
            axis = Axis.Vertical;
        }
        else if (Mathf.Abs(swipeLine.x) > Mathf.Abs(swipeLine.y))
        {
            axis = Axis.Horizontal;
        }

        return axis;
    }

    private void ResetFields()
    {
        // Resets
        currentDirection = Direction.None;
        CurrentAxis = Axis.None;
        longestSwipeLength = 0;
        startTime = 0;
        totalTouchTime = 0;
        IsTouchingScreen = false;
        currentDirection = default;
        TouchDelta = default;
        TouchLength = default;
        swipeMagnitude = 0;
    }
}