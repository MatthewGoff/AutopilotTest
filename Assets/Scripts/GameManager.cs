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
            || !float.TryParse(TurnRateField.GetComponent<InputField>().text, out TurnRate))
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
        bool thrustOutput = false;
        bool breakOutput = false;
        float turnOutput = 0f;

        Vector2 targetBearing = TargetPosition;
        Vector2 parallelVelocity = targetBearing * Vector2.Dot(targetBearing, Velocity) / Mathf.Pow(targetBearing.magnitude, 2f);
        Vector2 tangentVelocity = Velocity - parallelVelocity;
        float headingAngle = Vector2.SignedAngle(targetBearing, Heading);
        float tangentAngle = Vector2.SignedAngle(targetBearing, tangentVelocity);

        if (Vector2.Angle(targetBearing, Velocity) > 45f)
        {
            breakOutput = true;
        }
        if (headingAngle < 45f && Vector2.Dot(Heading, Velocity) < 0f)
        {
            thrustOutput = true;
        }
        if (headingAngle < 90f && Vector2.Dot(tangentVelocity, Heading) < 0f && tangentVelocity != Vector2.zero)
        {
            thrustOutput = true;
        }

        float turnRate = TurnRate * (2f * Mathf.PI / 360f);
        float thrustDuration = tangentVelocity.magnitude / Acceleration;
        float desiredHeadingAngle;
        if (turnRate * thrustDuration > 1f)
        {
            desiredHeadingAngle = 90f;
        }
        else
        {
            desiredHeadingAngle = (360f / (2f * Mathf.PI)) * Mathf.Acos(1 - turnRate * thrustDuration);
        }
        //desiredHeadingAngle = Mathf.Clamp(desiredHeadingAngle, -90f, 90f);
        if (tangentAngle > 0f)
        {
            desiredHeadingAngle *= -1;
        }

        turnOutput = headingAngle - desiredHeadingAngle;
        if (turnOutput < 0f)
        {
            turnOutput = -1f;
        }
        else if (turnOutput > 0f)
        {
            turnOutput = 1f;
        }

        DesiredHeading = desiredHeadingAngle;
        DesiredHeadingOutputText.GetComponent<Text>().text = "(DesiredHeading = "+ DesiredHeading.ToString()+")";
        ThrustOutputText.GetComponent<Text>().text = "Thrust: " + thrustOutput.ToString();
        BreakOutputText.GetComponent<Text>().text = "Break: " + breakOutput.ToString();
        TurnOutputText.GetComponent<Text>().text = "Turn: " + turnOutput.ToString();
    }
}
