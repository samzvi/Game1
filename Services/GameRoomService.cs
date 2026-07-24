using Game1.Models;
using System.Collections.Concurrent;

namespace Game1.Services;

/// <summary>
/// Manages in-memory game rooms: creation, joining, and reconnecting players.
/// Registered as a singleton.
/// </summary>
public class GameRoomService
{
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();

    public GameRoom CreateRoom(string nickname)
    {
        var pin = GeneratePin();
        var player = new Player { Nickname = nickname };
        var room = new GameRoom { Pin = pin, Player1 = player };

        _rooms[pin] = room;
        return room;
    }

    public (GameRoom? Room, string? Error) JoinOrReconnect(string pin, string nickname)
    {
        if (!_rooms.TryGetValue(pin, out var room))
        {
            return (null, "Room not found.");
        }

        if (room.IsFull)
        {
            return (null, "Room is full.");
        }

        var player = new Player { Nickname = nickname };
        if (room.Player1 == null)
        {
            room.Player1 = player;
        }
        else
        {
            room.Player2 = player;
        }

        room.NotifyChanged();
        return (room, null);
    }

    public GameRoom? TryGetRoom(string pin)
    {
        _rooms.TryGetValue(pin, out var room);
        return room;
    }

    public void MarkDisconnected(string pin, Guid playerId)
    {
        if (_rooms.TryGetValue(pin, out var room))
        {
            var player = room.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.IsConnected = false;
                room.NotifyChanged();
            }
        }
    }

    private string GeneratePin()
    {
        // Simple 4-digit numeric PIN for now, ensuring uniqueness in-memory
        var random = new Random();
        string pin;
        do
        {
            pin = random.Next(1000, 10000).ToString();
        } while (_rooms.ContainsKey(pin));

        return pin;
    }
}

