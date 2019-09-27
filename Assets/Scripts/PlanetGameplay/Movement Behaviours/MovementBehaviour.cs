﻿using UnityEngine;

[RequireComponent(typeof(IPhysicsController))]
public abstract class MovementBehaviour : MonoBehaviour
{
	public delegate void RollEventHandler(Vector3 direction);
	public event RollEventHandler OnRoll;

	private IPhysicsController physicsController;
	private IPhysicsController PhysicsController
		=> physicsController ?? (physicsController = GetComponent<IPhysicsController>());

	private static int wallLayer = -1;
	protected static int WallLayer => wallLayer == -1 ?
		wallLayer = LayerMask.NameToLayer("Wall")
		: wallLayer;
	private static int attackLayer = -1;
	protected static int AttackLayer => attackLayer == -1 ?
		attackLayer = LayerMask.NameToLayer("Attack")
		: attackLayer;

	[SerializeField] private float originalSpeed = 5f;

	public virtual void TriggerUpdate() { }

	protected virtual void OnCollisionEnter2D(Collision2D collision)
	{
		Collider2D otherCol = collision.collider;
		GameObject otherObj = otherCol.gameObject;
		int otherLayer = otherObj.layer;
		if (otherLayer == WallLayer)
		{
			IsTouchingWall = true;
		}
	}

	protected virtual void OnCollisionExit2D(Collision2D collision)
	{
		Collider2D otherCol = collision.collider;
		GameObject otherObj = otherCol.gameObject;
		int otherLayer = otherObj.layer;
		if (otherLayer == WallLayer)
		{
			IsTouchingWall = false;
		}
	}

	public bool IsTouchingWall { get; private set; }

	protected Vector3 SelfPosition => PhysicsController?.SelfPosition ?? transform.position;

	public float OriginalSpeed => originalSpeed;

	protected virtual float Speed => OriginalSpeed;

	public Vector3 MovementDirection => PhysicsController?.GetMovementDirection ?? Vector3.up;

	protected virtual float MovementSmoothingPower => 0f;

	protected void MoveTowardsPosition(Vector3 targetPosition)
		=> PhysicsController?.MoveTowardsPosition(targetPosition, Speed, MovementSmoothingPower);

	protected void MoveInDirection(Vector3 direction)
		=> PhysicsController?.MoveInDirection(direction, Speed);

	protected void MoveAwayFromPosition(Vector3 position)
		=> PhysicsController?.MoveAwayFromPosition(position, Speed);

	protected void SlowDown() => PhysicsController?.SlowDown();

	protected void Stop() => PhysicsController?.Stop();

	protected void SetVelocity(Vector3 direction)
		=> PhysicsController?.SetVelocity(direction);

	protected void FaceDirection(Vector3 direction)
		=> PhysicsController?.FaceDirection(direction);

	protected void TriggerRoll(Vector3 direction) => OnRoll?.Invoke(direction);
}
