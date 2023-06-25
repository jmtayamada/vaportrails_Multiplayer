using UnityEngine;
using System.Collections.Generic;
using Rewired;
using System.Linq;

public class PlayerInput : MonoBehaviour {
    Player player = null;
    Player player2 = null;

    Controller lastActiveController;

    [SerializeField] bool humanControl = false;
    public bool isHuman => humanControl;
    [SerializeField] bool human2Control = false;
    public bool isHuman2 => human2Control;

    public ComputerController comControl { get; private set; }

	void Awake() {
        player = ReInput.players.GetPlayer(0);
        player2 = ReInput.players.GetPlayer(2);
        comControl = new ComputerController();
    }

    void Start() {
        player.AddInputEventDelegate(ShowHideMouse, UpdateLoopType.Update);
        player.controllers.hasKeyboard = true;
        player.controllers.AddController(ControllerType.Joystick, 0, true);
        player2.AddInputEventDelegate(ShowHideMouse, UpdateLoopType.Update);
        player2.controllers.hasKeyboard = true;
    }

    void LateUpdate() {
        comControl.LateUpdate();
    }

    public Player GetPlayer() {
        return player;
    }

    public void EnableHumanControl() {
        humanControl = true;
    }

    public void DisableHumanControl() {
        humanControl = false;
    }

    public void EnableHuman2Control() {
        human2Control = true;
    }

    public void DisableHuman2Control() {
        human2Control = false;
    }

    void ShowHideMouse(InputActionEventData actionData) {
        if (!humanControl) return;
        lastActiveController = player.controllers.GetLastActiveController();
        Cursor.visible = (lastActiveController?.type == Rewired.ControllerType.Mouse);
    }

    public float GetAxis(int axisId) {
        if (humanControl) return player.GetAxis(axisId);
        if (human2Control) return player2.GetAxis(axisId);
        else return comControl.GetAxis(axisId);
    }

    public bool ButtonDown(int b) {
        if (humanControl) return player.GetButtonDown(b);
        if (human2Control) return player2.GetButtonDown(b);
        else return comControl.GetButtonDown(b);
    }

    public bool Button(int b) {
        if (humanControl) return player.GetButton(b);
        if (human2Control) return player2.GetButton(b);
        else return comControl.GetButton(b);
    }

    public bool ButtonUp(int b) {
        if (humanControl) return player.GetButtonUp(b);
        if (human2Control) return player2.GetButtonUp(b);
        else return comControl.GetButtonUp(b);
    }

    public bool HasHorizontalInput() {
        return HorizontalInput() != 0;
    }

    public float HorizontalInput() {
        return GetAxis(Buttons.H_AXIS);
    }

    public Vector2 UINav() {
        return new Vector2(
            GetAxis(Buttons.UI_X),
            GetAxis(Buttons.UI_Y)
        );
    }

    public float VerticalInput() {
        return GetAxis(Buttons.V_AXIS);
    }

    public bool GenericContinueInput() {
        return (
            ButtonDown(Buttons.JUMP) || GenericEscapeInput() || AttackInput()
        );
    }

    public bool AttackInput() {
        return (
            ButtonDown(Buttons.PUNCH)
            || ButtonDown(Buttons.KICK)
        );
    }

    public bool GenericEscapeInput() {
        return (
            ButtonDown(Buttons.SPECIAL)
            || ButtonDown(Buttons.INVENTORY)
            || Button(Buttons.PAUSE)
            || ButtonDown(Buttons.UI_CANCEL)
        );
    }

    public Vector2 LeftStick() {
        return new Vector2(
            GetAxis(Buttons.H_AXIS),
            GetAxis(Buttons.V_AXIS)
        );
    }

    public Vector2 MoveVector() {
        return new Vector2(HorizontalInput(), VerticalInput());
    }

    public static float GetInputBufferDuration() {
		return 2f/10f;
        // return SaveManager.save.options.inputBuffer * (1f/16f);
    }

    public static bool IsHorizontal(int actionID) {
		return actionID == RewiredConsts.Action.Horizontal;
	}

    public static bool IsAttack(int actionID) {
        return 
            actionID == RewiredConsts.Action.Punch
            || actionID == RewiredConsts.Action.Kick
            || actionID == RewiredConsts.Action.Projectile
        ;
    }

    public static PlayerInput GetPlayerOneInput() {
		return GameObject.FindObjectsOfType<PlayerInput>()
			.Where(x => x.humanControl)
			.First();
	}
    
    public static PlayerInput GetPlayerTwoInput() {
		return GameObject.FindObjectsOfType<PlayerInput>()
			.Where(x => x.human2Control)
			.First();
	}
}

public static class Buttons {
    public static readonly int H_AXIS = RewiredConsts.Action.Horizontal;
    public static readonly int V_AXIS = RewiredConsts.Action.Vertical;
    public static readonly int JUMP   = RewiredConsts.Action.Jump;
    public static readonly int PUNCH  = RewiredConsts.Action.Punch;
    public static readonly int KICK   = RewiredConsts.Action.Kick;
    public static readonly int INVENTORY = RewiredConsts.Action.Inventory;

    public static readonly int SPECIAL    = RewiredConsts.Action.Special;
    public static readonly int PROJECTILE = RewiredConsts.Action.Projectile;
    public static readonly int INTERACT   = RewiredConsts.Action.Interact;
    public static readonly int PARRY      = RewiredConsts.Action.Parry;

    public static readonly int PAUSE   = RewiredConsts.Action.Start;
    public static readonly int TABLEFT = RewiredConsts.Action.TabLeft;
    public static readonly int TABRIGHT = RewiredConsts.Action.TabRight;

    public static readonly int UI_X = RewiredConsts.Action.UIHorizontal;
    public static readonly int UI_Y = RewiredConsts.Action.UIVertical;
    public static readonly int UI_CANCEL = RewiredConsts.Action.UICancel;
    public static readonly int UI_SUBMIT = RewiredConsts.Action.UISubmit;
}
