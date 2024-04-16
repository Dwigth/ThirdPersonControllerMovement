using Godot;

public partial class TargetFollower : Node3D
{
	[Export]
	protected Node3D m_Target;
	[Export]
	private bool _AutoTargetPlayer;
	protected RigidBody3D targetRigidbody;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_Awake();
		if (_AutoTargetPlayer)
		{
			FindAndTargetPlayer();
		}
		if (m_Target == null) return;
		//targetRigidbody = GetNode<RigidBody3D>("RigidBody3D");
	}

	public override void _PhysicsProcess(double delta)
	{
		FollowTarget(delta);
	}

	protected virtual void FollowTarget(double deltaTime) { }
	protected virtual void _Awake() { }

	public void FindAndTargetPlayer()
	{
		// auto target an object tagged player, if no target has been assigned
		Node3D targetObj = GetNode<Node3D>("Player");
		if (targetObj != null)
		{
			SetTarget(targetObj);
		}
	}


	public virtual void SetTarget(Node3D newTarget)
	{
		m_Target = newTarget;
	}


	public Node3D Target
	{
		get { return m_Target; }
	}

}
