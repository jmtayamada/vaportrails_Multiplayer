using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CombatController : MonoBehaviour, IAttackLandListener, IHitListener {	
	AttackHitbox attackHitbox;

	protected WallCheckData wallData;
	protected Rigidbody2D rb2d;
	protected GroundData groundData;
	protected EntityController player;
	protected Animator animator;
	protected PlayerAttackGraph currentGraph;
	protected PlayerInput input;

	public PlayerAttackGraph groundAttackGraph;
	public PlayerAttackGraph airAttackGraph;

	public float diStrength = 3f;

	const float techWindow = 0.3f;
	const float techLockoutLength = 0.6f;
	bool canTech = false;
	bool techLockout = false;
	GameObject techEffect;
	Collider2D collider2d;

	protected virtual void Start() {
		player = GetComponent<EntityController>();
		groundData = GetComponent<GroundCheck>().groundData;
		rb2d = GetComponent<Rigidbody2D>();
		wallData = GetComponent<WallCheck>().wallData;
		animator = GetComponent<Animator>();
		attackHitbox = GetComponentInChildren<AttackHitbox>();
		input = GetComponent<PlayerInput>();
		collider2d = GetComponent<Collider2D>();

		techEffect = Resources.Load<GameObject>("Runtime/TechEffect");

		groundAttackGraph.Initialize(
			this,
			animator,
			GetComponent<AttackBuffer>(),
			GetComponent<AirAttackTracker>(),
			input
		);
		airAttackGraph.Initialize(
			this,
			animator,
			GetComponent<AttackBuffer>(),
			GetComponent<AirAttackTracker>(),
			input
		);
	}

	public void OnAttackLand(Hurtbox hurtbox) {
		if (currentGraph) currentGraph.OnAttackLand();
	}

	protected virtual void Update() {
		if (!player.frozeInputs && currentGraph == null) {
			CheckAttackInputs();
		}

		if (currentGraph != null) {
			currentGraph.UpdateGrounded(groundData.grounded);
			currentGraph.Update();
		}

		if (groundData.hitGround || wallData.hitWall) {
			RefreshAirAttacks();
		}

		if (!techLockout && player.stunned && !canTech) {
			if (input.ButtonDown(Buttons.SPECIAL)) {
				canTech = true;
				Invoke(nameof(EndTechWindow), techWindow);
			}
		}

		CheckForTech();
	}

	protected virtual void CheckAttackInputs() {
		if (input.ButtonDown(Buttons.PUNCH) || input.ButtonDown(Buttons.KICK)) {
			if (groundData.grounded) {
				EnterAttackGraph(groundAttackGraph);
			} else if (!wallData.touchingWall) {
				EnterAttackGraph(airAttackGraph);
			}
		}
	}

	void CheckForTech() {
		if (player.stunned && (groundData.hitGround || wallData.hitWall)) {
			if (!techLockout && canTech) {
				OnTech();
			}
		}
	}

	protected virtual void OnTech() {
		if (wallData.touchingWall) {
			rb2d.velocity = Vector2.zero;
			player.RefreshAirMovement();
			RefreshAirAttacks();
			Instantiate(
				techEffect,
				transform.position + new Vector3(wallData.direction * collider2d.bounds.extents.x, 0, 0),
				Quaternion.identity,
				null
			);
		} else if (groundData.grounded) {
			rb2d.velocity = new Vector2(
				player.movement.runSpeed * Mathf.Sign(input.HorizontalInput()),
				0
			);
			Instantiate(
				techEffect,
				transform.position + Vector3.down*collider2d.bounds.extents.y,
				Quaternion.identity,
				null
			);
		}
		animator.SetTrigger("TechSuccess");
		GetComponent<EntityShader>().FlashCyan();
		canTech = false;
		CancelInvoke(nameof(EndTechWindow));
		player.CancelStun();
	}

	void EndTechWindow() {
		canTech = false;
		techLockout = true;
		this.WaitAndExecute(() => techLockout = false, techLockoutLength);
	}

	public void OnHit(AttackHitbox attack) {
		// sideways DI is stronger than towards/away
		// (sin(2x - (1/4 circle))) * 0.4 + 0.6
		// ↑ this is a sinewave between 0.2 and 1.0 that peaks at (1, 0) and (-1, 0)
		Vector2 selfKnockback = player.GetKnockback(attack);
		Vector2 leftStick = input.LeftStick();
		float angle = Vector2.SignedAngle(selfKnockback, leftStick);
		float diMagnitude = (Mathf.Cos(angle * Mathf.Deg2Rad)* 0.4f) + 0.6f;
		rb2d.velocity += leftStick * diMagnitude * diStrength;
	}

	public virtual void EnterAttackGraph(PlayerAttackGraph graph, CombatNode entryNode=null) {
		player.OnAttackGraphEnter();
		currentGraph = graph;
		graph.EnterGraph(entryNode);
	}

	public void OnAttackNodeEnter(CombatNode combatNode) {
		if (combatNode is AttackNode) {
			AttackNode attackNode = combatNode as AttackNode;
			player.OnAttackNodeEnter(attackNode.attackData);
			attackHitbox.data = attackNode.attackData;
		} else {
			player.OnAttackNodeEnter(null);
		}
	}

	public void OnAttackNodeExit() {
		player.OnAttackNodeExit();
	}

	public void OnGraphExit() {
		player.OnAttackGraphExit();
		currentGraph = null;
	}

	public float GetSpeed() {
		return Mathf.Abs(rb2d.velocity.x);
	}

	public bool IsSpeeding() {
		return player.IsSpeeding();
	}

	virtual public void RefreshAirAttacks() {

	}
}
