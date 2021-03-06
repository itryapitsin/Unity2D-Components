using Matcha.Dreadful;
using Matcha.Unity;
using UnityEngine;

public class CreatureEntity : Entity
{
	public enum EntityType { Enemy };
	public EntityType entityType;
	public int hp;
	public int ac;
	public int touchDamage;

	private BoxCollider2D thisCollider;
	private Weapon playerWeapon;
	private BreakableManager breakable;
	private bool dead;
	private bool blockedRight;
	private bool blockedLeft;
	private float repulseMin  = .3f;
	private float repulseMax  = .8f;
	private float repulseTime = .2f;

	void Start()
	{
		breakable = gameObject.GetComponentInChildren<BreakableManager>();

		if (entityType == EntityType.Enemy) { AutoAlign(); }
	}

	private void SetBlockedRightState(bool status)
	{
		blockedRight = status;
	}

	private void SetBlockedLeftState(bool status)
	{
		blockedLeft = status;
	}

	override public void OnWeaponCollisionEnter(Collider2D coll)
	{
		if (!dead)
		{
			playerWeapon = coll.GetComponentInParent<Weapon>();

			hitFrom = M.HorizSideThatWasHit(gameObject, coll);

			if (playerWeapon.weaponType == Weapon.WeaponType.Hammer ||
					playerWeapon.weaponType == Weapon.WeaponType.Dagger ||
					playerWeapon.weaponType == Weapon.WeaponType.MagicProjectile)
			{
				TakesProjectileHit(playerWeapon, coll, hitFrom);
			}
			else if (playerWeapon.weaponType == Weapon.WeaponType.Axe ||
					playerWeapon.weaponType == Weapon.WeaponType.Sword)
			{
				TakesMeleeHit(playerWeapon, coll, hitFrom);
			}
		}
	}

	void TakesMeleeHit(Weapon playerWeapon, Collider2D coll, int hitFrom)
	{
		if (!dead)
		{
			hp -= (int)(playerWeapon.damage);

			// bounceback from projectile
			if (hitFrom == RIGHT && !blockedLeft)
			{
				MFX.RepulseToLeftRandomly(transform, repulseMin, repulseMax, repulseTime);
			}
			else if (hitFrom == LEFT && !blockedRight)
			{
				MFX.RepulseToRightRandomly(transform, repulseMin, repulseMax, repulseTime);
			}
			else
			{
				rigidbody2D.velocity = Vector2.zero;
			}

			if (hp <= 0)
			{
				EventKit.Broadcast<int>("prize collected", worth);
				KillSelf(hitFrom, MELEE);
			}
		}
	}

	void TakesProjectileHit(Weapon playerWeapon, Collider2D coll, int hitFrom)
	{
		if (!dead)
		{
			hp -= (int)(playerWeapon.damage);

			// bounceback from projectile
			if (hitFrom == RIGHT && !blockedLeft)
			{
				// rigidbody2D.AddForce(new Vector3(-100, 0, 0));
				MFX.RepulseToLeftRandomly(transform, .3f, .8f, .2f);
			}
			else if (hitFrom == LEFT && !blockedRight)
			{
				// rigidbody2D.AddForce(new Vector3(100, 0, 0));
				MFX.RepulseToRightRandomly(transform, .3f, .8f, .2f);
			}
			else
			{
				rigidbody2D.velocity = Vector2.zero;
			}

			if (hp <= 0)
			{
				EventKit.Broadcast<int>("prize collected", worth);
				KillSelf(hitFrom, PROJECTILE);
			}
		}
	}

	void KillSelf(int hitFrom, int weaponType)
	{
		if (!dead)
		{
			// activate and kill breakable sprite
			if (weaponType == MELEE)
			{
				breakable.DirectionalSlump(hitFrom);
			}
			else if (weaponType == PROJECTILE)
			{
				breakable.Explode(hitFrom);
			}

			// deactivate and fade solid sprite
			rigidbody2D.isKinematic   = true;
			collider2D.enabled        = false;

			MFX.Fade(spriteRenderer, 0f, 0f, 0f);

			dead = true;

			gameObject.SendMessage("CreatureDead");

			Invoke("DeactivateObject", MAX_BEFORE_FADE + 5f);
		}
	}

	void DeactivateObject()
	{
		gameObject.SetActive(false);
	}

	override public void OnBodyCollisionEnter(Collider2D coll) {}
	override public void OnBodyCollisionStay() {}
	override public void OnBodyCollisionExit() {}
	override public void OnWeaponCollisionStay() {}
	override public void OnWeaponCollisionExit() {}
}
