namespace Game1.Models;

/// <summary>
/// Represents a single game room shared by up to two players.
/// </summary>
public class GameRoom
{
    public required string Pin { get; init; }

    public GameSettings Settings { get; init; } = new();

    public Player? Player1 { get; set; }

    public Player? Player2 { get; set; }

    public IEnumerable<Player> Players
    {
        get
        {
            if (Player1 is not null) yield return Player1;
            if (Player2 is not null) yield return Player2;
        }
    }

    public bool IsFull => Player1 is not null && Player2 is not null;

    /// <summary>
    /// Raised whenever the room's state changes (player joined, reconnected, disconnected, etc.).
    /// Subscribers should call StateHasChanged (via InvokeAsync if needed) to refresh their UI.
    /// </summary>
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();
}
