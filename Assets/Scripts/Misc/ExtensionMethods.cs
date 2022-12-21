using UnityEngine;

public static class ExtensionMethods
{
    /// <summary>
    /// It's in the name.
    /// </summary>
    /// <param name="parent">Murder this Transform's children</param>
    public static void DestroyAllChildren(this Transform parent)
    {
        int nrOfChildren = parent.childCount;

        for (int i = 0; i < nrOfChildren; i++)
        {
            Transform child = parent.GetChild(i);
            GameObject.Destroy(child.gameObject);
        }
    }

    public static void SetChildrenActivityState(this Transform parent, bool state)
    {
        int nrOfChildren = parent.childCount;

        for (int i = 0; i < nrOfChildren; i++)
        {
            Transform child = parent.GetChild(i);
            child.gameObject.SetActive(state);
        }
    }


    public static Vector2 Abs(this Vector2 vector) => new(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
}