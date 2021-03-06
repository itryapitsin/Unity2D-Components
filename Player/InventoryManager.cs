using DG.Tweening;
using Matcha.Dreadful;
using Matcha.Unity;
using UnityEngine;

public class InventoryManager : CacheBehaviour
{
	private Weapon equippedWeapon;
	private Weapon leftWeapon;
	private Weapon rightWeapon;

	private int left = 0;
	private int equipped = 1;
	private int right = 2;
	private bool levelLoading;
	private GameObject[] weaponBelt;
	private GameObject outgoingWeapon;

	void OnInitWeapons(GameObject eWeapon, GameObject lWeapon, GameObject rWeapon)
	{
		// WEAPON GAMEOBJECTS — keep track of weapon GameObjects as they're equipped/stashed
		if (weaponBelt == null) {
			weaponBelt = new GameObject[3];
		}

		weaponBelt[left]     = lWeapon;
		weaponBelt[equipped] = eWeapon;
		weaponBelt[right]    = rWeapon;

		InitNewEquippedWeapon(eWeapon);
		PassInitialWeaponsToHUD();
		PassEquippedWeaponToWeaponManager();
	}

	void OnEquipNewWeapon(GameObject newWeapon)
	{
		DiscardWeapon();
		InitNewEquippedWeapon(newWeapon);
	}

	void DiscardWeapon()
	{
		outgoingWeapon = weaponBelt[equipped];
		outgoingWeapon.layer = PICKUP_LAYER;
		outgoingWeapon.GetComponentInChildren<PhysicsCollider>().EnablePhysicsCollider();
		outgoingWeapon.GetComponentInChildren<MeleeCollider>().DisableMeleeCollider();
		outgoingWeapon.GetComponentInChildren<Rigidbody2D>().isKinematic = false;
		outgoingWeapon.GetComponent<SpriteRenderer>().enabled = true;
		outgoingWeapon.GetComponent<SpriteRenderer>().sortingLayerName = PICKUP_SORTING_LAYER;
		outgoingWeapon.transform.SetLocalPositionXY(0f, .5f);
		outgoingWeapon.transform.SetAbsLocalScaleX(1f);
		outgoingWeapon.transform.parent = null;
		equippedWeapon.inPlayerInventory = false;
		equippedWeapon.inEnemyInventory = false;
		TossOutgoingWeapon();
		Invoke("EnablePickupCollider", 1f);
	}

	void InitNewEquippedWeapon(GameObject newWeapon)
	{
		weaponBelt[equipped] = newWeapon;
		weaponBelt[equipped].layer = PLAYER_LAYER;
		weaponBelt[equipped].transform.parent = gameObject.transform;
		weaponBelt[equipped].transform.localPosition = new Vector3(0f, 0f, .1f);
		weaponBelt[equipped].transform.SetLocalScaleXYZ(-1f, 1f, 1f);
		weaponBelt[equipped].GetComponent<SpriteRenderer>().enabled = true;
		weaponBelt[equipped].GetComponentInChildren<PhysicsCollider>().DisablePhysicsCollider();
		weaponBelt[equipped].GetComponentInChildren<MeleeCollider>().EnableMeleeCollider();
		weaponBelt[equipped].GetComponentInChildren<WeaponPickupCollider>().DisableWeaponPickupCollider();
		weaponBelt[equipped].GetComponentInChildren<Rigidbody2D>().isKinematic = true;
		weaponBelt[equipped].GetComponent<Weapon>().inPlayerInventory = true;
		weaponBelt[equipped].GetComponent<Weapon>().inEnemyInventory = false;
		CacheAndSetupWeapons();
		PassEquippedWeaponToHUD();
		PassEquippedWeaponToWeaponManager();
	}

	void EnablePickupCollider()
	{
		//turn dropped weapon's pickup collider back on after a short delay
		outgoingWeapon.GetComponentInChildren<WeaponPickupCollider>().EnableWeaponPickupCollider();
	}

	void TossOutgoingWeapon()
	{
		if (transform.lossyScale.x > 0f) {
			outgoingWeapon.GetComponent<Rigidbody2D>().AddForce(new Vector3(-5, 5, 1), ForceMode2D.Impulse);
		}
		else
		{
			outgoingWeapon.GetComponent<Rigidbody2D>().AddForce(new Vector3(5, 5, 1), ForceMode2D.Impulse);
		}
	}

	void OnSwitchWeapon(int shiftDirection)
	{
		if (!levelLoading)
		{
			switch (equipped)
			{
				case 0:
				{
					left = 1;
					equipped = 2;
					right = 0;
					break;
				}

				case 1:
				{
					left = 2;
					equipped = 0;
					right = 1;
					break;
				}

				case 2:
				{
					left = 0;
					equipped = 1;
					right = 2;
					break;
				}
			}

			CacheAndSetupWeapons();
			PassNewWeaponsToHUD();
			PassEquippedWeaponToWeaponManager();
		}
	}

	void CacheAndSetupWeapons()
	{
		Profiler.BeginSample("CacheAndSetupWeapons >> InventoryManager.cs");

		// WEAPON GAMEOBJECT'S 'WEAPON' COMPONENT
		// ~~~~~~~~~~~~~~~~~~~~~~~~
		// cache specific weapons (Sword, Hammer, etc) via parent class 'Weapon'
		// use to call currently equipped weapon animations
		leftWeapon       = weaponBelt[left].GetComponent<Weapon>();
		equippedWeapon   = weaponBelt[equipped].GetComponent<Weapon>();
		rightWeapon      = weaponBelt[right].GetComponent<Weapon>();

		// set correct names for weapon gameObjects
		weaponBelt[left].name = "Left";
		weaponBelt[equipped].name = "Equipped";
		weaponBelt[right].name = "Right";

		// set weapons to correct layers
		weaponBelt[left].layer = PLAYER_LAYER;
		weaponBelt[equipped].layer = PLAYER_LAYER;
		weaponBelt[right].layer = PLAYER_LAYER;

		// set weapons to correct sorting layers;
		weaponBelt[left].GetComponent<SpriteRenderer>().sortingLayerName = HERO_WEAPON_SORTING_LAYER;
		weaponBelt[equipped].GetComponent<SpriteRenderer>().sortingLayerName = HERO_WEAPON_SORTING_LAYER;
		weaponBelt[right].GetComponent<SpriteRenderer>().sortingLayerName = HERO_WEAPON_SORTING_LAYER;

		// set weapon colliders to correct layers
		weaponBelt[left].transform.Find("MeleeCollider").gameObject.layer = WEAPON_COLLIDER;
		weaponBelt[equipped].transform.Find("MeleeCollider").gameObject.layer = WEAPON_COLLIDER;
		weaponBelt[right].transform.Find("MeleeCollider").gameObject.layer = WEAPON_COLLIDER;

		// set pickup colliders to correct layers
		weaponBelt[left].transform.Find("PickupCollider").gameObject.layer = PICKUP_LAYER;
		weaponBelt[equipped].transform.Find("PickupCollider").gameObject.layer = PICKUP_LAYER;
		weaponBelt[right].transform.Find("PickupCollider").gameObject.layer = PICKUP_LAYER;

		// disable SpriteRenderer for weapons that are not equipped
		leftWeapon.spriteRenderer.enabled = false;
		equippedWeapon.spriteRenderer.enabled = true;
		rightWeapon.spriteRenderer.enabled = false;

		// disable colliders for all weapons - they are only enabled during attacks
		leftWeapon.GetComponent<BoxCollider2D>().enabled = false;
		equippedWeapon.GetComponent<BoxCollider2D>().enabled = false;
		rightWeapon.GetComponent<BoxCollider2D>().enabled = false;

		// fade in newly equipped weapon
		float fadeAfter = 0f;
		float fadeTime  = .2f;

		SpriteRenderer equippedSprite  = weaponBelt[equipped].GetComponent<SpriteRenderer>();
		equippedSprite.DOKill();
		MFX.Fade(equippedSprite, 1f, fadeAfter, fadeTime);

		// fade out newly stashed weapons
		FadeOutStashedWeapons(leftWeapon);
		FadeOutStashedWeapons(rightWeapon);

		SendMessageUpwards("NewWeaponEquipped", equippedWeapon.weaponType);

		Profiler.EndSample();
	}

	void FadeOutStashedWeapons(Weapon stashedWeapon)
	{
		float fadeAfter = 0f;
		float fadeTime  = .2f;

		SpriteRenderer stashedSprite  = stashedWeapon.GetComponent<SpriteRenderer>();
		stashedSprite.DOKill();
		MFX.Fade(stashedSprite, 0f, fadeAfter, fadeTime);
	}

	void PassInitialWeaponsToHUD()
	{
		EventKit.Broadcast<GameObject, int>("init stashed weapon", weaponBelt[left], LEFT);
		EventKit.Broadcast<GameObject>("init equipped weapon", weaponBelt[equipped]);
		EventKit.Broadcast<GameObject, int>("init stashed weapon", weaponBelt[right], RIGHT);
	}

	void PassNewWeaponsToHUD()
	{
		EventKit.Broadcast<GameObject, int>("change stashed weapon", weaponBelt[left], LEFT);
		EventKit.Broadcast<GameObject>("change equipped weapon", weaponBelt[equipped]);
		EventKit.Broadcast<GameObject, int>("change stashed weapon", weaponBelt[right], RIGHT);
	}

	void PassEquippedWeaponToHUD()
	{
		EventKit.Broadcast<GameObject>("init new equipped weapon", weaponBelt[equipped]);
	}

	void PassEquippedWeaponToWeaponManager()
	{
		EventKit.Broadcast<Weapon>("new equipped weapon", equippedWeapon);
	}

	void OnLevelLoading(bool status)
	{
		// pause weapon changes while level loading
		levelLoading = true;

		StartCoroutine(Timer.Start(WEAPON_PAUSE_ON_LEVEL_LOAD, false, () => {
			levelLoading = false;
		}));
	}

	void OnEnable()
	{
		EventKit.Subscribe<GameObject, GameObject, GameObject>("init weapons", OnInitWeapons);
		EventKit.Subscribe<GameObject>("equip new weapon", OnEquipNewWeapon);
		EventKit.Subscribe<int>("switch weapon", OnSwitchWeapon);
		EventKit.Subscribe<bool>("level loading", OnLevelLoading);
	}

	void OnDestroy()
	{
		EventKit.Unsubscribe<GameObject, GameObject, GameObject>("init weapons", OnInitWeapons);
		EventKit.Unsubscribe<GameObject>("equip new weapon", OnEquipNewWeapon);
		EventKit.Unsubscribe<int>("switch weapon", OnSwitchWeapon);
		EventKit.Unsubscribe<bool>("level loading", OnLevelLoading);
	}
}
