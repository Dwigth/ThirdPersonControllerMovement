using Godot;
public partial class FreeLookCam : PivotBasedCameraRig
{
    [Export] private double m_MoveSpeed = 1;                      // How fast the rig will move to keep up with the target's position.
    [Export] private float m_TurnSpeed = 1.5f;                    // How fast the rig will rotate from user input. 0f - 10f
    [Export] private float m_TurnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
    [Export] private float m_TiltMax = 75f;                       // The maximum value of the x axis rotation of the pivot.
    [Export] private float m_TiltMin = 45f;                       // The minimum value of the x axis rotation of the pivot.
    [Export] private bool m_LockCursor = false;                   // Whether the cursor should be hidden and locked.

    private float m_LookAngle;                    // The rig's y axis rotation.
    private float m_TiltAngle;                    // The pivot's x axis rotation.
    private const float k_LookDistance = 100f;    // How far in front of the pivot the character's look target is.
    private Vector3 m_PivotEulers;
    private Quaternion m_PivotTargetRot;
    private Quaternion m_TransformTargetRot;
    private double timeScaled = 0;
    private double deltaTime = 0;

    protected override void _Awake()
    {
        base._Awake();
        // TODO: Lock or unlock the cursor.

        m_PivotEulers = m_Pivot.Transform.Basis.GetEuler();

        m_PivotTargetRot = m_Pivot.Basis.GetRotationQuaternion();
        m_TransformTargetRot = Transform.Basis.GetRotationQuaternion();
    }

    public override void _Process(double delta)
    {
        deltaTime = delta;
        HandleRotationMovement();
    }

    protected override void FollowTarget(double deltaTime)
    {
        if (m_Target == null) return;
        Transform = Transform.InterpolateWith(m_Target.Transform, (float)(deltaTime * m_MoveSpeed));
    }

    private void HandleRotationMovement()
    {
        timeScaled = Engine.TimeScale;
        if (timeScaled < float.Epsilon)
        {
            return;
        }
        float x = Input.GetActionStrength("strafe_left") - Input.GetActionStrength("strafe_right");
        float y = Input.GetActionStrength("strafe_back") - Input.GetActionStrength("strafe_forward");

        m_LookAngle += x * 0.0001f * m_TurnSpeed;

        m_TransformTargetRot = new Quaternion(Vector3.Up, Mathf.RadToDeg(m_LookAngle));


        //m_Target.Quaternion = m_TransformTargetRot;



        m_TiltAngle -= y * 0.001f * m_TurnSpeed;
        m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);

        Vector3 xRotation = Vector3.Right;
        xRotation.Y = m_Pivot.RotationDegrees.Y;
        xRotation.Z = m_Pivot.RotationDegrees.Z;

        m_PivotTargetRot = new Quaternion(xRotation, m_TiltAngle);

        if (m_TurnSmoothing > 0f)
        {
            m_Pivot.Quaternion = new Quaternion().Slerp(m_PivotTargetRot, (float)(m_TurnSmoothing * deltaTime)).Normalized();
            Quaternion = new Quaternion().Slerp(m_TransformTargetRot, (float)(m_TurnSmoothing * deltaTime)).Normalized();
        }
        else
        {
            m_Pivot.Quaternion = m_PivotTargetRot;
            Quaternion = m_TransformTargetRot;
        }
    }


}