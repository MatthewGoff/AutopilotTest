using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public RectTransform UI;

    public GameObject TargetPositionXField;
    public GameObject TargetPositionYField;
    public GameObject VelocityXField;
    public GameObject VelocityYField;
    public GameObject HeadingXField;
    public GameObject HeadingYField;
    public GameObject AccelerationField;
    public GameObject TurnRateField;
    public GameObject MaxSpeedField;

    public GameObject ToggleGroup;
    public GameObject TargetPositionToggle;
    public GameObject VelocityToggle;
    public GameObject HeadingToggle;

    public GameObject WarningText;

    public GameObject DesiredHeadingOutputText;
    public GameObject ThrustOutputText;
    public GameObject BreakOutputText;
    public GameObject TurnOutputText;

    public Vector2 TargetPosition { get; private set; }
    public Vector2 Velocity { get; private set; }
    public Vector2 Heading { get; private set; }
    public float DesiredHeading { get; private set; }
    private float Acceleration;
    private float TurnRate;
    private float MaxSpeed;

    public bool ValidInputs { get; private set; }

    private void Awake()
    {
        instance = this;
    }
	
	private void Update ()
    {
        HandleClickEvents();
        ValidInputs = GatherInputs();

        if (ValidInputs)
        {
            WarningText.SetActive(false);
            RunAutopilot();
        }
        else
        {
            WarningText.SetActive(true);
            DesiredHeadingOutputText.GetComponent<Text>().text = "(DesiredHeading = -)";
            ThrustOutputText.GetComponent<Text>().text = "Thrust: -";
            BreakOutputText.GetComponent<Text>().text = "Break: -";
            TurnOutputText.GetComponent<Text>().text = "Turn: -";
        }
	}

    private void HandleClickEvents()
    {
        if (Input.GetMouseButton(0) && Input.mousePosition.x > UI.rect.width)
        {
            Vector2 clickPosition = CameraController.GetMousePosition();
            Toggle toggleGroup = ToggleGroup.GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();
            if (toggleGroup == TargetPositionToggle.GetComponent<Toggle>())
            {
                TargetPositionXField.GetComponent<InputField>().text = clickPosition.x.ToString();
                TargetPositionYField.GetComponent<InputField>().text = clickPosition.y.ToString();
            }
            else if (toggleGroup == VelocityToggle.GetComponent<Toggle>())
            {
                VelocityXField.GetComponent<InputField>().text = clickPosition.x.ToString();
                VelocityYField.GetComponent<InputField>().text = clickPosition.y.ToString();
            }
            else if (toggleGroup == HeadingToggle.GetComponent<Toggle>())
            {
                HeadingXField.GetComponent<InputField>().text = clickPosition.x.ToString();
                HeadingYField.GetComponent<InputField>().text = clickPosition.y.ToString();
            }
        }
    }
    
    private bool GatherInputs()
    {
        float targetPositionX = 0f;
        float targetPositionY = 0f;
        float velocityX = 0f;
        float velocityY = 0f;
        float headingX = 0f;
        float headingY = 0f;
        if (!float.TryParse(TargetPositionXField.GetComponent<InputField>().text, out targetPositionX)
            || !float.TryParse(TargetPositionYField.GetComponent<InputField>().text, out targetPositionY)
            || !float.TryParse(VelocityXField.GetComponent<InputField>().text, out velocityX)
            || !float.TryParse(VelocityYField.GetComponent<InputField>().text, out velocityY)
            || !float.TryParse(HeadingXField.GetComponent<InputField>().text, out headingX)
            || !float.TryParse(HeadingYField.GetComponent<InputField>().text, out headingY)
            || !float.TryParse(AccelerationField.GetComponent<InputField>().text, out Acceleration)
            || !float.TryParse(TurnRateField.GetComponent<InputField>().text, out TurnRate)
            || !float.TryParse(MaxSpeedField.GetComponent<InputField>().text, out MaxSpeed))
        {
            return false;
        }
        else
        {
            TargetPosition = new Vector2(targetPositionX, targetPositionY);
            Velocity = new Vector2(velocityX, velocityY);
            Heading = new Vector2(headingX, headingY);
            return true;
        }
    }

    private void RunAutopilot()
    {
        float turnOutput = 0;
        bool thrustOutput = false;
        bool breakOutput = false;

        Vector2 targetVector = TargetPosition - new Vector2(0, 0);
        Vector2 perpendicularTargetVector = new Vector2(targetVector.y, -targetVector.x);
        Vector2 parallelVelocity = targetVector * Vector2.Dot(targetVector, Velocity) / Mathf.Pow(targetVector.magnitude, 2f);
        Vector2 perpendicularVelocity = Velocity - parallelVelocity;
        Vector2 nextVelocity = Velocity + (Heading.normalized * Acceleration * Time.fixedDeltaTime);
        Vector2 perpendicularNextVelocity = perpendicularTargetVector * Vector2.Dot(perpendicularTargetVector, Velocity) / Mathf.Pow(perpendicularTargetVector.magnitude, 2f);
        float headingAngle = Vector2.SignedAngle(targetVector, Heading);
        float perpendicularAngle = Vector2.SignedAngle(targetVector, perpendicularVelocity);

        if (Vector2.Angle(targetVector, Velocity) > 45f)
        {
            breakOutput = true;
        }
        if (Mathf.Abs(headingAngle) < 45f
            && Vector2.Dot(Heading, Velocity) < 0f)
        {
            thrustOutput = true;
        }
        if (Mathf.Abs(headingAngle) < 90f
            && Vector2.Dot(perpendicularVelocity, Heading) < 0f
            && Vector2.Dot(perpendicularNextVelocity, perpendicularVelocity) >= 0f)
        {
            thrustOutput = true;
        }

        if (Mathf.Abs(headingAngle) < 1f)
        {
            if (Velocity.magnitude < MaxSpeed)
            {
                thrustOutput = true;
            }
            else
            {
                thrustOutput = false;
            }
        }

        float turnRate = TurnRate * (2f * Mathf.PI / 360f);
        float thrustDuration = perpendicularVelocity.magnitude / Acceleration;
        float desiredHeadingAngle;
        if (turnRate * thrustDuration > 1f)
        {
            desiredHeadingAngle = 90f;
        }
        else
        {
            desiredHeadingAngle = (360f / (2f * Mathf.PI)) * Mathf.Acos(1 - turnRate * thrustDuration);
        }
        if (perpendicularAngle > 0f)
        {
            desiredHeadingAngle *= -1;
        }

        if (Mathf.Abs(desiredHeadingAngle) < 15f)
        {
            desiredHeadingAngle = 0f;
        }

        turnOutput = desiredHeadingAngle - headingAngle;
        if (turnOutput < -180f)
        {
            turnOutput += 360f;
        }
        else if (turnOutput > 180f)
        {
            turnOutput -= 360f;
        }

        turnOutput = Mathf.Clamp(turnOutput, -TurnRate * Time.fixedDeltaTime, TurnRate * Time.fixedDeltaTime);
        turnOutput /= TurnRate * Time.fixedDeltaTime;

        DesiredHeading = desiredHeadingAngle;
        DesiredHeadingOutputText.GetComponent<Text>().text = "(DesiredHeading = "+ DesiredHeading.ToString()+")";
        ThrustOutputText.GetComponent<Text>().text = "Thrust: " + thrustOutput.ToString();
        BreakOutputText.GetComponent<Text>().text = "Break: " + breakOutput.ToString();
        TurnOutputText.GetComponent<Text>().text = "Turn: " + turnOutput.ToString();
    }
}
