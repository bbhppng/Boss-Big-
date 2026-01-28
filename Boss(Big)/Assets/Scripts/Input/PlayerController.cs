using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerController", menuName = "InputController/PlayerController")]
public class PlayerController : InputController
{
    private PlayerInputActions _inputActions;
    private bool _isJumping, _isAttacking, _isInteracting, _isPullingHero, _isPullingHuzz, _isBlocking, _isRequestingHeal;
    
    private void OnEnable()
    {
        _inputActions = new PlayerInputActions();
        _inputActions.Player.Enable();
        _inputActions.Player.Jump.started += JumpStarted;
        _inputActions.Player.Jump.canceled += JumpCanceled;
        _inputActions.Player.Attack.started += AttackStarted;
        _inputActions.Player.Attack.canceled += AttackCanceled;
        _inputActions.Player.Interact.started += InteractStarted;
        _inputActions.Player.Interact.canceled += InteractCanceled;
        _inputActions.Player.HeroPull.started += HeroPullStarted;
        _inputActions.Player.HeroPull.canceled += HeroPullCanceled;
        _inputActions.Player.HuzzPull.started += HuzzPullStarted;
        _inputActions.Player.HuzzPull.canceled += HuzzPullCanceled;
        _inputActions.Player.Block.started += BlockStarted;
        _inputActions.Player.Block.canceled += BlockCanceled;
        _inputActions.Player.Heal.started += HealStarted;
        _inputActions.Player.Heal.canceled += HealCanceled;
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
        _inputActions.Player.Jump.started -= JumpStarted;
        _inputActions.Player.Jump.canceled -= JumpCanceled;
        _inputActions.Player.Attack.started -= AttackStarted;
        _inputActions.Player.Attack.canceled -= AttackCanceled;
        _inputActions.Player.Interact.started -= InteractStarted;
        _inputActions.Player.Interact.canceled -= InteractCanceled;
        _inputActions.Player.HeroPull.started -= HeroPullStarted;
        _inputActions.Player.HeroPull.canceled -= HeroPullCanceled;
        _inputActions.Player.HuzzPull.started -= HuzzPullStarted;
        _inputActions.Player.HuzzPull.canceled -= HuzzPullCanceled;
        _inputActions.Player.Block.started -= BlockStarted;
        _inputActions.Player.Block.canceled -= BlockCanceled;
        _inputActions.Player.Heal.started -= HealStarted;
        _inputActions.Player.Heal.canceled -= HealCanceled;
        _inputActions = null;
    }
    private void HealStarted(InputAction.CallbackContext obj)
    {
        _isRequestingHeal = true;
    }
    private void HealCanceled(InputAction.CallbackContext obj)
    {
        _isRequestingHeal = false;
    }
    private void BlockStarted(InputAction.CallbackContext obj)
    {
        _isBlocking = true;
    }
    private void BlockCanceled(InputAction.CallbackContext obj)
    {
        _isBlocking = false;
    }

    private void HuzzPullStarted(InputAction.CallbackContext obj)
    {
        _isPullingHuzz = true;
    }
    private void HuzzPullCanceled(InputAction.CallbackContext obj)
    {
        _isPullingHuzz = false;
    }
    
    private void HeroPullStarted(InputAction.CallbackContext obj)
    {
        _isPullingHero = true;
    }
    private void HeroPullCanceled(InputAction.CallbackContext obj)
    {
        _isPullingHero = false;
    }
    private void InteractCanceled(InputAction.CallbackContext obj)
    {
        _isInteracting = false;
    }
    private void InteractStarted(InputAction.CallbackContext obj)
    {
        _isInteracting = true;
    }

    private void JumpCanceled(InputAction.CallbackContext obj)
    {
        _isJumping = false;
    }

    private void JumpStarted(InputAction.CallbackContext obj)
    {
        _isJumping = true;
    }

    private void AttackCanceled(InputAction.CallbackContext obj)
    {
        _isAttacking = false;
    }

    private void AttackStarted(InputAction.CallbackContext obj)
    {
        _isAttacking = true;
    }

    public override float RetrieveMoveInput()
    {
        return _inputActions.Player.Move.ReadValue<Vector2>().x;
    }

    public override bool RetrieveJumpInput()
    {
        return _isJumping;
    }

    public override bool RetrieveAttackInput()
    {
        return _isAttacking;
    }

    public override bool RetrieveInteractInput()
    {
        return _isInteracting;
    }

    public override bool RetrieveHeroPullInput()
    {
        return _isPullingHero;
    }

    public override bool RetrieveHuzzPullInput()
    {
        return _isPullingHuzz;
    }

    public override bool RetrieveBlockInput()
    {
        return _isBlocking;
    }
    public override bool RetrieveHealInput()
    {
        return _isRequestingHeal;
    }
}