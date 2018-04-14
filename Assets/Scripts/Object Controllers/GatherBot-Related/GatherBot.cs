﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherBot : Entity, IDrillableObject, IDamageable
{
	private enum AIState
	{
		//exit the hive and find a position to begin scanning
		Spawning,
		//looking for resources to gather
		Scanning,
		//found resources, now trying to harvest
		Gathering,
		//didn't find resources, now exploring a bit to scan a different area
		Exploring,
		//inventory full, returning to hive to store findings
		Storing,
		//scan found an unknown entity, determing threat level
		Suspicious,
		//calling for reinforcements
		Signalling,
		//attacking unknown entity
		Attacking
	}

	//references
	private BotHive hive;
	[SerializeField]
	private ShakeEffect shakeFX;

	//fields
	[SerializeField]
	private AIState state;
	private float maxHealth = 500f;
	private float currentHealth;

	//movement variables
	private Vector2 accel;
	[SerializeField]
	private float engineStrength = 1f, speedLimit = 2f, deceleration = 2f, decelEffectiveness = 0.01f,
		rotationSpeed = 3f;
	private float rot = 180f;
	private Vector2 velocity;
	private Entity targetEntity;
	private float maxSway = 45;
	private float distanceCheck = 10f;

	//scanning variables
	[SerializeField]
	private ExpandingCircle scanningBeam;
	private float scanTimer, scanDuration = 3f;
	private bool scanStarted;

	private void Start()
	{
		state = AIState.Spawning;
		transform.eulerAngles = Vector3.forward * -rot;
		initialised = true;
	}

	private void Update()
	{
		accel = Vector2.zero;

		switch (state)
		{
			default:
			case AIState.Spawning:
				Spawning();
				break;
			case AIState.Scanning:
				Scanning();
				break;
			case AIState.Gathering:
				Gathering();
				break;
			case AIState.Exploring:
				Exploring();
				break;
			case AIState.Storing:
				Storing();
				break;
			case AIState.Suspicious:
				Suspicious();
				break;
			case AIState.Signalling:
				Signalling();
				break;
			case AIState.Attacking:
				Attacking();
				break;
		}

		ApplyMovementCalculation();
	}

	private void FixedUpdate()
	{
		Rb.AddForce(accel);
	}

	#region State Methods

	private void Spawning()
	{
		Vector2 targetPos = hive.transform.position + Vector3.down * 2f;
		float distLeft = Vector2.Distance(transform.position, targetPos);
		if (distLeft > 1f)
		{
			float expectedAngle = DetermineDirection(targetPos);
			float speedMod = 1f - RotateTo(expectedAngle);
			DetermineAcceleration(speedMod, distLeft);
		}
		else
		{
			state = AIState.Scanning;
		}

	}

	private void Scanning()
	{
		if (Rb.velocity.sqrMagnitude < Mathf.Epsilon)
		{
			if (!scanStarted)
			{
				StartCoroutine(ScanRings());
				scanStarted = true;
			}
			else
			{
				scanTimer += Time.deltaTime;
			}

			if (scanTimer >= scanDuration)
			{
				scanTimer = 0f;
				scanStarted = false;

				//choose state
			}
		}
	}

	private void Gathering()
	{

	}

	private void Exploring()
	{

	}

	private void Storing()
	{

	}

	private void Suspicious()
	{

	}

	private void Signalling()
	{

	}

	private void Attacking()
	{

	}

	#endregion

	private void DetermineAcceleration(float speedMod, float distLeft)
	{
		if (distLeft <= 3f)
		{
			speedMod *= distLeft / 3f;
		}
		float mag = engineStrength * speedMod;
		float topAccel = Mathf.Min(engineStrength, speedLimit);
		accel = transform.up * Mathf.Min(mag, topAccel);
	}

	private float RotateTo(float angle)
	{
		float rotMod = Mathf.Abs(rot - angle);
		if (rotMod > 180f)
		{
			rotMod = Mathf.Abs(rotMod - 360f);
		}
		rotMod /= 180f;
		rotMod = Mathf.Pow(rotMod, 0.8f);
		rot = (Mathf.MoveTowardsAngle(rot, angle, rotationSpeed * rotMod) + 360f) % 360f;
		return rotMod;
	}

	private float DetermineDirection(Vector2 targetPos)
	{
		float angleTo = Vector2.Angle(Vector2.up, targetPos - (Vector2)transform.position);
		if (targetPos.x < transform.position.x)
		{
			angleTo = 180f + (180f - angleTo);
		}

		return RaycastDivert(angleTo, Vector2.Distance(transform.position, targetPos));
	}

	private float RaycastDivert(float angleTo, float DistLeft)
	{
		Vector2[] dirs = new Vector2[]
		{
			new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo - 30f)), Mathf.Cos(Mathf.Deg2Rad * (angleTo - 30f))),
			new Vector2(Mathf.Sin(Mathf.Deg2Rad * angleTo), Mathf.Cos(Mathf.Deg2Rad * angleTo)),
			new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo + 30f)), Mathf.Cos(Mathf.Deg2Rad * (angleTo + 30f)))
		};
		RaycastHit2D[] hits = new RaycastHit2D[dirs.Length];
		float dist = Mathf.Min(distanceCheck, DistLeft);
		for (int i = 0; i < dirs.Length; i++)
		{
			dirs[i].Normalize();
			dirs[i] *= dist;
			Debug.DrawLine((Vector2)transform.position + dirs[i] / (dist * 2f),
				(Vector2)transform.position + dirs[i]);
			hits[i] = Physics2D.Raycast((Vector2)transform.position + dirs[i] / (dist * 2f),
				dirs[i], dist);
		}

		float change = 0f;

		for (int i = 0; i < hits.Length; i++)
		{
			RaycastHit2D hit = hits[i];

			if (hit.collider != null && !IsTarget(hit))
			{
				accel *= hit.fraction;
				float delta = 1f - hit.fraction;
				if (hit.rigidbody != null)
				{
					switch (i)
					{
						case 0:
							change += maxSway * delta;
							break;
						case 1:
							Vector2 normal = hit.normal;
							float normalAngle = Vector2.Angle(Vector2.up, normal);
							bool moveRight = Mathf.MoveTowardsAngle(angleTo, normalAngle, 1f) > angleTo;
							if (moveRight)
							{
								change += maxSway * delta * 2f;
							}
							else
							{
								change -= maxSway * delta * 2f;
							}
							break;
						case 2:
							change -= maxSway * delta;
							break;
					}
				}
			}
		}

		Debug.DrawLine(transform.position, (Vector2)transform.position
			+ new Vector2(Mathf.Sin(Mathf.Deg2Rad * (angleTo + change)), Mathf.Cos(Mathf.Deg2Rad * (angleTo + change))), Color.red);

		return angleTo + change;
	}

	private bool IsTarget(RaycastHit2D hit)
	{
		if (targetEntity == null) return false;
		foreach (Collider2D col in targetEntity.Col)
		{
			if (hit.collider == col) return true;
		}
		return false;
	}

	private void ApplyMovementCalculation()
	{
		float speedCheck = CheckSpeed();
		float decelerationModifier = 1f;
		if (speedCheck > 1f)
		{
			decelerationModifier *= speedCheck;
		}

		if (IsDrilling)
		{
			//freeze constraints
			Rb.constraints = RigidbodyConstraints2D.FreezeAll;
			//add potential velocity
			velocity += accel / 10f;
			//apply a continuous slowdown effect
			velocity = Vector3.MoveTowards(velocity, Vector3.zero, 0.1f);
			//set an upper limit so that the drill speed doesn't go too extreme
			if (speedCheck > 1f)
			{
				velocity.Normalize();
				velocity *= speedLimit;
			}
			rot = -transform.eulerAngles.z;
		}
		else
		{
			velocity = Rb.velocity;
			//keep constraints unfrozen
			Rb.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
			//apply deceleration
			Rb.drag = Mathf.MoveTowards(
				Rb.drag,
				deceleration * decelerationModifier,
				decelEffectiveness);
			transform.eulerAngles = Vector3.forward * -rot;
		}
	}

	private IEnumerator ScanRings()
	{
		WaitForSeconds wfs = new WaitForSeconds(0.5f);
		System.Action a = () =>
		{
			ExpandingCircle scan = Instantiate(scanningBeam);
			scan.lifeTime = scanDuration;
			scan.transform.position = transform.position;
		};
		a();
		yield return wfs;
		a();
		yield return wfs;
		a();
	}

	private float CheckSpeed()
	{
		return velocity.magnitude / speedLimit;
	}

	public void Create(BotHive botHive, float MaxHP)
	{
		hive = botHive;
		state = AIState.Scanning;
		maxHealth = MaxHP;
		currentHealth = maxHealth;
	}

	public bool TakeDrillDamage(float drillDmg, Vector2 drillPos)
	{
		return TakeDamage(drillDmg, drillPos);
	}

	public void StartDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.FreezeAll;
		shakeFX.Begin();
	}

	public void StopDrilling()
	{
		Rb.constraints = RigidbodyConstraints2D.None;
		shakeFX.Stop();
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();
			if (otherDrill.CanDrill)
			{
				StartDrilling();
				otherDrill.StartDrilling(this);
			}
		}

		if (otherLayer == layerProjectile)
		{
			IProjectile projectile = other.GetComponent<IProjectile>();
			projectile.Hit(this);
		}
	}

	public void OnTriggerExit2D(Collider2D other)
	{
		int otherLayer = other.gameObject.layer;

		if (otherLayer == layerDrill)
		{
			DrillBit otherDrill = other.GetComponentInParent<DrillBit>();

			if ((Entity)otherDrill.drillTarget == this)
			{
				otherDrill.StopDrilling();
			}
		}
	}

	public bool TakeDamage(float damage, Vector2 damagePos)
	{
		currentHealth -= damage;

		return CheckHealth();
	}

	private bool CheckHealth()
	{
		return currentHealth <= 0f;
	}

	public override float DrillDamageQuery(bool firstHit)
	{
		return speedLimit;
	}
}