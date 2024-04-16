using Godot;
public partial class PivotBasedCameraRig : TargetFollower
{

    protected Node3D m_Cam;
    protected Node3D m_Pivot;
    protected Vector3 m_LastTargetPosition;

    protected override void _Awake()
    {
        m_Pivot = GetNode<Node3D>("Pivot");
        m_Cam = m_Pivot.GetNode<Node3D>("Camera3D");
    }
}