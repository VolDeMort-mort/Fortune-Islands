public abstract class GameState
{
    protected GameManager game; // Access to the core data

    public GameState(GameManager gameManager)
    {
        this.game = gameManager;
    }

    public virtual void Enter() { }  // What happens when phase starts?
    public virtual void Update() { } // What happens every frame?
    public virtual void Exit() { }   // What happens when phase ends?
}