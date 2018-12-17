using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    private static readonly float MIN_CAMERA_HEIGHT = 1f;
    private static readonly float MAX_CAMERA_HEIGHT = 60f;
    private static readonly float MAX_CAMERA_WIDTH = 120f;
    private static readonly float ZOOM_SPEED = 1f;//1.2f;
    private static readonly float PAN_SPEED = 0f;//1f;

    public static CameraController instance;

    public RectTransform UI;
    private Camera Camera;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        Camera = GetComponent<Camera>();
        float ppu = Camera.pixelWidth / (2 * Camera.orthographicSize * Camera.aspect);
        float uiWidth = UI.rect.width;
        float cameraOffset = -uiWidth / (2 * ppu);
        transform.position += new Vector3(cameraOffset, 0, 0);
    }

    private void Update()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        Camera.transform.position += PAN_SPEED * movement;

        var d = Input.GetAxis("Mouse ScrollWheel");
        if (d > 0f)
        {
            Camera.orthographicSize *= (1f / ZOOM_SPEED);
            if (Camera.orthographicSize < 1f)
            {
                Camera.orthographicSize = 1f;
            }
        }
        else if (d < 0f)
        {
            Camera.orthographicSize *= ZOOM_SPEED;
            if (Camera.orthographicSize > 30f)
            {
                Camera.orthographicSize = 30f;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public static Vector2 GetMousePosition()
    {
        if (instance == null)
        {
            return Vector2.zero;
        }
        else
        {
            return instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnPostRender()
    {
        DrawGridLines();
        if (GameManager.instance.ValidInputs)
        {
            DrawVectorLines();
        }
    }

    private void DrawVectorLines()
    {
        Vector2 targetPosition = GameManager.instance.TargetPosition;
        Vector2 velocity = GameManager.instance.Velocity;
        Vector2 tangentVelocity = targetPosition * Vector2.Dot(targetPosition, velocity) / Mathf.Pow(targetPosition.magnitude, 2);
        Vector2 parallelVelocity = velocity - tangentVelocity;
        Vector2 heading = GameManager.instance.Heading;
        Vector2 desiredHeading;
        if (GameManager.instance.DesiredHeading != GameManager.instance.DesiredHeading)
        {
            desiredHeading = Vector2.zero;
        }
        else
        {
            float targetAngle = Vector2.SignedAngle(Vector2.right, targetPosition);
            desiredHeading = Quaternion.Euler(0f, 0f, GameManager.instance.DesiredHeading + targetAngle) * Vector2.right;
        }

        Material lineMat = new Material(Shader.Find("Sprites/Default"));
        GL.Begin(GL.LINES);
        lineMat.SetPass(0);

        // Target Position
        GL.Color(new Color(1f, 0f, 0f, 1f));
        GL.Vertex3(0f, 0f, 0f);
        GL.Vertex3(targetPosition.x, targetPosition.y, 0f);

        // Velocity
        GL.Color(new Color(0f, 1f, 0f, 1f));
        GL.Vertex3(0f, 0f, 0f);
        GL.Vertex3(velocity.x, velocity.y, 0f);
        GL.Vertex3(0f, 0f, 0f);
        GL.Vertex3(tangentVelocity.x, tangentVelocity.y, 0f);
        GL.Vertex3(0f, 0f, 0f);
        GL.Vertex3(parallelVelocity.x, parallelVelocity.y, 0f);

        // Heading
        GL.Color(new Color(0f, 0f, 1f, 1f));
        GL.Vertex3(0f, 0f, 0f);
        GL.Vertex3(heading.x, heading.y, 0f);

        // Desired Heading
        GL.Color(new Color(0f, 1f, 1f, 1f));
        GL.Vertex3(0f, 0f, 0f);
        GL.Vertex3(desiredHeading.x, desiredHeading.y, 0f);
        GL.End();
    }

    private void DrawGridLines()
    {
        float height = 2f * Camera.orthographicSize;
        float width = height * Camera.aspect;
        float top = transform.position.y + height / 2f;
        float bottom = transform.position.y - height / 2f;
        float left = transform.position.x - width / 2f;
        float right = transform.position.x + width / 2f;

        Material lineMat = new Material(Shader.Find("Sprites/Default"));
        GL.Begin(GL.LINES);
        lineMat.SetPass(0);
        Draw1sGrid(top, bottom, left, right);
        Draw10sGrid(top, bottom, left, right);
        GL.End();
    }

    private void Draw1sGrid(float top, float bottom, float left, float right)
    {
        GL.Color(new Color(1f, 1f, 1f, 0.2f));

        for (int i = Mathf.CeilToInt(left); i < right; i++)
        {
            GL.Vertex3(i, bottom, 0f);
            GL.Vertex3(i, top, 0f);
        }

        for (int i = Mathf.CeilToInt(bottom); i < top; i++)
        {
            GL.Vertex3(left, i, 0f);
            GL.Vertex3(right, i, 0f);
        }
    }

    private void Draw10sGrid(float top, float bottom, float left, float right)
    {
        GL.Color(new Color(1f, 1f, 1f, 1f));

        for (int i = Mathf.CeilToInt(left / 10) * 10; i < right; i += 10)
        {
            GL.Vertex3(i, bottom, 0f);
            GL.Vertex3(i, top, 0f);
        }

        for (int i = Mathf.CeilToInt(bottom / 10) * 10; i < top; i += 10)
        {
            GL.Vertex3(left, i, 0f);
            GL.Vertex3(right, i, 0f);
        }
    }

}
