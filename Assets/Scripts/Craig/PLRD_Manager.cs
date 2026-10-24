using UnityEngine;

public class PLRD_Manager : MonoBehaviour
{
    PLRD_Enum currentEnum;
    Vector2 offset;

    public float swayAmount = 2f;     //speed of sway
    public float maxSwayAngle = 25f;   
    public float swaySmoothing = 10f;  // low smooth , high jumpy

    Vector3 lastPosition;
    float currentAngle;

    public BoxCollider2D boxCollider;

    void Start()
    {
        currentEnum = PLRD_Enum.Placed;
        lastPosition = transform.position;
    }

    void Update()
    {
        /*switch (currentEnum)
        {
            case PLRD_Enum.Placed:
                //PLRD_Drag();
                break;

            case PLRD_Enum.Dragging:
                PLRD_Drag();
                break;

            case PLRD_Enum.Locked:
                //PLRD_Drag();
                break;
        }*/

        if (currentEnum == PLRD_Enum.Dragging)
        {
            PLRD_Drag();
        }

        ApplySway();
    }

    Vector2 GetMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void PLRD_Drag()
    {
        transform.position = GetMousePos() + offset;
    }


    void ApplySway()
    {
        // Horizontal 
        float velocityX = (transform.position.x - lastPosition.x) / Time.deltaTime;
        lastPosition = transform.position;

        float targetAngle = Mathf.Clamp(-velocityX * swayAmount, -maxSwayAngle, maxSwayAngle);

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, swaySmoothing * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }


    private void OnMouseDown()
    {
        Debug.Log("Clicking");
        offset = (Vector2)transform.position - GetMousePos();

        if (currentEnum != PLRD_Enum.Locked)
        {
            currentEnum = PLRD_Enum.Dragging;
            LockHitBox(false);
        }
        else
        {
            // noise
        }
        
    }

    private void OnMouseUp()
    {
        if (currentEnum != PLRD_Enum.Locked)
        {
            currentEnum = PLRD_Enum.Placed;
            LockHitBox(true);
        }
    }

    private void LockHitBox(bool boxBool)
    {
        if (boxBool == true)
        {
            boxCollider.enabled = true;
        }

        else
        {
            boxCollider.enabled = false;
        }
            
    }

    public void Lock()
    {
        currentEnum = PLRD_Enum.Locked;
    }
}
