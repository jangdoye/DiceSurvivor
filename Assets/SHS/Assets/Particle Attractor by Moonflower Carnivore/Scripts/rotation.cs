using UnityEngine;

public class rotation : MonoBehaviour
{


    #region Variables
    public Transform topEnd; // 따라갈 대상

    #endregion

    #region Unity Event Method
    void OnEnable()
    {
        InvokeRepeating("rotate", 0f, 0.0167f); // 약 60fps
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    public void clickOn()
    {
        InvokeRepeating("rotate", 0f, 0.0167f);
    }

    public void clickOff()
    {
        CancelInvoke();
    }
    private void rotate()
    {
        if (topEnd != null)
        {
            transform.position = topEnd.position;
            transform.rotation = topEnd.rotation;
        }
    }

    #endregion

}
