using Godot;
using Godot.Collections;
using System.Diagnostics;

public partial class CharacterMovement : CharacterBody3D
{

	[Export] private Camera3D m_Cam;
	[Export] private bool m_PlayerHasComplexModel;
	private Vector3 m_CamForward;
	public Vector3 m_Move;
	private SpringArm3D m_SpringArm;
	private Node3D m_Model;
	private AnimationTree m_AnimationTree;
	private AnimationNodeStateMachinePlayback m_AnimationState;
	private float m_Rotationm_Speed = 0.01f;
	private bool m_Jumping = false;
	private Array m_Attacks = new()
	{
		"1H_Melee_Attack_Slice_Diagonal",
		"1H_Melee_Attack_Chop",
		"1H_Melee_Attack_Slice_Horizontal"
	};

	private bool m_LastFloor = true;
	private bool m_IsAttacking = false;
	public const float m_Speed = 5.0f;
	public const float m_JumpVelocity = 4.5f;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		if (!m_PlayerHasComplexModel) return;
		m_Model = GetNode<Node3D>("Rig");
		m_AnimationTree = GetNode<AnimationTree>("AnimationTree");
		m_AnimationState = (AnimationNodeStateMachinePlayback)m_AnimationTree.Get("parameters/playback");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = m_JumpVelocity;
			m_Jumping = true;
			m_AnimationTree.Set("parameters/conditions/jumping", true);
			m_AnimationTree.Set("parameters/conditions/grounded", false);
		}
		if (IsOnFloor() && m_LastFloor == false)
		{
			m_Jumping = false;
			m_AnimationTree.Set("parameters/conditions/jumping", false);
			m_AnimationTree.Set("parameters/conditions/grounded", true);
		}
		if (!IsOnFloor() && m_Jumping == false)
		{
			m_AnimationState.Travel("Jump_Idle");
			m_AnimationTree.Set("parameters/conditions/grounded", false);
		}
		m_LastFloor = IsOnFloor();


		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		m_CamForward = (m_Cam.GlobalBasis.Z * new Vector3(1, 0, 1)).Normalized();
		//CharacterMovement should be set by the model forward
		m_Move = Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

		if (m_Move != Vector3.Zero)
		{
			velocity.X = m_Move.X * m_Speed;
			velocity.Z = m_Move.Z * m_Speed;

			m_Model.LookAt(m_Move + Position);

			Vector3 velocityBlenPosition = velocity * Transform.Basis;
			m_AnimationTree.Set("parameters/IWR/blend_position", new Vector2(velocityBlenPosition.X, -velocityBlenPosition.Z) / m_Speed);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, m_Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, m_Speed);
			Vector3 velocityBlenPosition = velocity * Transform.Basis;
			m_AnimationTree.Set("parameters/IWR/blend_position", new Vector2(velocityBlenPosition.X, -velocityBlenPosition.Z) / m_Speed);
		}

		Velocity = velocity;
		MoveAndSlide();

		if (velocity.Length() > 1.0f)
		{
			Transform3D transform = m_Model.Transform;
			Vector3 axis = transform.Basis.GetEuler();
			axis.Y = Mathf.LerpAngle(m_Model.Rotation.Y, m_Move.Y, (float)(m_Rotationm_Speed * delta));
			transform.Basis = new Basis(axis, 0.01f) * transform.Basis;
			m_Model.Transform = transform;
		}

		if (Input.IsActionPressed("attack"))
		{
			m_IsAttacking = true;
		}

		while (m_IsAttacking)
		{
			StringName attack = (StringName)m_Attacks.PickRandom();
			m_AnimationState.Travel(attack);
			m_IsAttacking = false;
		}
	}
}
