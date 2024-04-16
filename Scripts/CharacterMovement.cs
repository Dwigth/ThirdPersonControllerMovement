using Godot;
using System;
using System.Diagnostics;

public partial class CharacterMovement : CharacterBody3D
{

	[Export] private Camera3D m_Cam;
	private Vector3 m_CamForward;
	public Vector3 m_Move;
	private float m_TurnAmount;
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		m_CamForward = (m_Cam.GlobalBasis.Z * new Vector3(1, 0, 1)).Normalized();
		m_Move = inputDir.Y * m_CamForward + inputDir.X * m_Cam.GlobalTransform.Basis.X;

		if (m_Move.Length() > 1) m_Move = m_Move.Normalized();
		//Debug.WriteLine(m_Move.Length());
		if (m_Move != Vector3.Zero)
		{
			velocity.X = m_Move.X * Speed;
			velocity.Z = m_Move.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
			m_TurnAmount = 0;
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
