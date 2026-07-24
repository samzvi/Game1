namespace Game1.Models;

/// <summary>
/// Represents a single player within a game room.
/// </summary>
public class Player
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Nickname { get; set; }

    public bool IsConnected { get; set; } = true;
}
