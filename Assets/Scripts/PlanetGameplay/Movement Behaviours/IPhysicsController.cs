﻿using System;
using UnityEngine;

public interface IPhysicsController
{
	void MoveInDirection(Vector3 direction, float speed);
	void MoveTowardsPosition(Vector3 position, float speed, float smoothingPower);
	void MoveAwayFromPosition(Vector3 position, float speed);
	void Stop();
	void SlowDown();
	Vector3 SelfPosition { get; }
	void SetVelocity(Vector3 direction);
	bool CanMove { get; }
	void PreventMovementInputForDuration(float duration);
	Vector3 GetMovementDirection { get; }
	Vector3 GetFacingDirection { get; }
	void FaceDirection(Vector3 direction);
	bool EnableCollider { get; set; }
	void DeactivateColliderForDuration(float duration);
}
