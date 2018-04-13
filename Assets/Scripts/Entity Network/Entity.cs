﻿using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	[Header("Entity Fields")]
	[SerializeField]
	protected ChunkCoords _coords;
	public Collider2D[] Col;
	public Rigidbody2D Rb;
	public bool ShouldDisablePhysicsOnDistance = true;
	public bool ShouldDisableObjectOnDistance = true;
	public bool ShouldDisableGameObjectOnShortDistance = true;
	public bool isActive = true;
	private bool disabled = false;
	private Vector3 vel;
	private float disableTime;
	protected bool needsInit = true;
	protected bool initialised = false;

	//related layers
	private static bool layersSet;
	protected static int layerDrill, layerProjectile, layerSolid;

	//drill related
	public bool canDrill;
	private DrillBit drill;
	public bool IsDrilling { get { return drill == null ? false : drill.IsDrilling; } }

	//components to disable/enable
	public List<MonoBehaviour> ScriptComponents;
	public Renderer[] rends;

	private static int entitiesActive;

	public static int GetActive()
	{
		return entitiesActive;
	}

	public virtual void Awake()
	{
		entitiesActive++;
		_coords = new ChunkCoords(transform.position);
		EntityNetwork.AddEntity(this, _coords);
		GetLayers();
	}

	public virtual void LateUpdate()
	{
		RepositionInNetwork();
	}

	public void RepositionInNetwork()
	{
		ChunkCoords newCc = new ChunkCoords(transform.position);
		if (newCc != _coords)
			EntityNetwork.Reposition(this, newCc);

		if (needsInit && !initialised) return;

		SetAllActivity(IsInView());
		if (ShouldDisablePhysicsOnDistance)
		{
			if (IsInPhysicsRange())
			{
				if (!disabled) return;
				entitiesActive++;
				disabled = false;
				gameObject.SetActive(true);
				if (Rb != null)
				{
					Rb.simulated = true;
				}
				PhysicsReEnabled();
			}
			else
			{
				if (disabled) return;
				entitiesActive--;
				disabled = true;
				vel = Rb == null ? vel : (Vector3)Rb.velocity;
				if (Rb != null)
				{
					Rb.simulated = !ShouldDisablePhysicsOnDistance;
				}
				gameObject.SetActive(!ShouldDisableObjectOnDistance);
			}
		}
	}

	public void SetCoordinates(ChunkCoords newCc)
	{
		_coords = newCc;
	}

	protected bool IsInView()
	{
		return CameraCtrl.IsCoordInView(_coords);
	}

	protected bool IsInPhysicsRange()
	{
		return CameraCtrl.IsCoordInPhysicsRange(_coords);
	}

	public void SetAllActivity(bool active)
	{
		if (active == isActive || !ShouldDisableObjectOnDistance) return;
		if (needsInit && !initialised) return;

		isActive = active;

		foreach (Renderer r in rends)
		{
			if (r != null)
			{
				r.enabled = active;
			}
		}

		if (ShouldDisableGameObjectOnShortDistance)
		{
			gameObject.SetActive(active);
		}

		//enable/disable all relevant components
		foreach (MonoBehaviour script in ScriptComponents)
		{
			if (script != null)
			{
				script.enabled = active;
			}
		}
	}

	public virtual EntityType GetEntityType()
	{
		return EntityType.Entity;
	}

	public virtual void DestroySelf()
	{
		if (EntityNetwork.ConfirmLocation(this, _coords))
		{
			EntityNetwork.RemoveEntity(this);
		}
		Destroy(gameObject);
	}

	public ChunkCoords GetCoords()
	{
		return _coords;
	}

	public override string ToString()
	{
		return string.Format("{0} at coordinates {1}.", GetEntityType(), _coords);
	}

	public DrillBit GetDrill()
	{
		return canDrill ? drill : null;
	}

	public void SetDrill(DrillBit newDrill)
	{
		canDrill = newDrill != null;
		drill = newDrill;
	}

	public void AttachDrill(DrillBit db)
	{
		drill = db;
	}

	//This should be overridden. Called by a drill to determine how much damage it should deal to its target.
	public virtual float DrillDamageQuery(bool firstHit)
	{
		return 1f;
	}

	public virtual void PhysicsReEnabled()
	{

	}

	private void GetLayers()
	{
		if (layersSet) return;

		layerDrill = LayerMask.NameToLayer("Drill");
		layerProjectile = LayerMask.NameToLayer("Projectile");
		layerSolid = LayerMask.NameToLayer("Solid");

		layersSet = true;
	}
}

public enum EntityType
{
	Entity,
	Asteroid,
	Shuttle,
	Nebula
}