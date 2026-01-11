using UnityEngine;

public abstract class InputController : ScriptableObject
{
    public abstract float RetrieveMoveInput();
    public abstract bool RetrieveJumpInput();
    public abstract bool RetrieveAttackInput();
    public abstract bool RetrieveHeroPullInput();
    public abstract bool RetrieveHuzzPullInput();
    public abstract bool RetrieveInteractInput();
    //public abstract bool RetrieveDashInput();
    //public abstract Vector2 RetrieveMousePositionInput();
}