using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackHitbox : MonoBehaviour {
	public bool attacksPlayer;
	public AttackData data;
	IAttackLandListener[] attackLandListeners;
	Collider2D[] colliders;
	HashSet<Hurtbox> hitThisActive = new HashSet<Hurtbox>();
	bool hitboxOutLastFrame = false;

	void Start() {
		gameObject.layer = LayerMask.NameToLayer(Layers.Hitboxes);
		attackLandListeners = GetComponentsInParent<IAttackLandListener>();
		colliders = GetComponents<Collider2D>();
	}

	void Update() {
		bool hitboxOut = false;
		foreach (Collider2D collider in colliders) {
			if (collider.enabled) {
				hitboxOut = true;
				break;
			}
		}

		if (!hitboxOut && hitboxOutLastFrame) {
			hitThisActive.Clear();
		}

		hitboxOutLastFrame = hitboxOut;
	}

	protected virtual bool CanHit(Hurtbox hurtbox) {
		if (hurtbox.gameObject.CompareTag(Tags.Player) && !attacksPlayer) return false;
		if (hitThisActive.Contains(hurtbox)) return false;
		return true;
	}

	void OnTriggerEnter2D(Collider2D other) {
		Hurtbox hurtbox = other.GetComponent<Hurtbox>();
		if (hurtbox && CanHit(hurtbox)) {
			Hitstop.Run(data.hitstop);

			foreach (Hurtbox h in hurtbox.transform.root.GetComponentsInChildren<Hurtbox>()) {
				hitThisActive.Add(h);
			}

			Collider2D currentActiveCollider = colliders[0];
			foreach (Collider2D col in colliders) {
				if (col.enabled) currentActiveCollider = col;
			}

			if (data.hitSound) data.hitSound.PlayFrom(gameObject);
			if (data.hitmarker) {
				Instantiate(data.hitmarker, currentActiveCollider.ClosestPoint(other.transform.position), Quaternion.identity);
				foreach (IAttackLandListener attackLandListener in attackLandListeners) {
					attackLandListener.OnAttackLand(hurtbox);
				}
			}
			hurtbox.OnAttackLand(this);
		}
	}
}
