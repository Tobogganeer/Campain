

public interface IInteractable
{
    public void StartInteract(PlayerPawn player);

    public float GetInteractTime();

    public void FinishInteract(PlayerPawn player);

    public void EndInteract(PlayerPawn player);

    public bool CanInteract(PlayerPawn player);
}