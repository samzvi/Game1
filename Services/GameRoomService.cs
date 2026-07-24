using Game1.Models;
using System.Collections.Concurrent;

namespace Game1.Services;

public enum JoinRoomError
{
    None,
    NotFound,
    RoomFull,
    NicknameTooShort,
    NicknameTooLong
}

/// <summary>
/// Manages in-memory game rooms: creation, joining, and reconnecting players.
/// Registered as a singleton.
/// </summary>
public class GameRoomService
{
    public const int MinNicknameLength = 3;
    public const int MaxNicknameLength = 20;

    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();

    public (GameRoom? Room, JoinRoomError Error) CreateRoom(string nickname)
    {
        var nicknameError = ValidateNickname(nickname);
        if (nicknameError != JoinRoomError.None)
        {
            return (null, nicknameError);
        }

        var pin = GeneratePin();
        var player = new Player { Nickname = nickname.Trim() };
        var room = new GameRoom { Pin = pin, Player1 = player };

        _rooms[pin] = room;
        return (room, JoinRoomError.None);
    }

    public (GameRoom? Room, Player? Player, JoinRoomError Error) JoinOrReconnect(string pin, string nickname)
    {
        var nicknameError = ValidateNickname(nickname);
        if (nicknameError != JoinRoomError.None)
        {
            return (null, null, nicknameError);
        }

        nickname = nickname.Trim();

        if (!_rooms.TryGetValue(pin, out var room))
        {
            return (null, null, JoinRoomError.NotFound);
        }

        // Reconnect an existing (disconnected) player with the same nickname instead of
        // creating a brand-new one, so rejoining doesn't fill the room with duplicates.
        var existing = room.Players.FirstOrDefault(p => !p.IsConnected && p.Nickname == nickname);
        if (existing is not null)
        {
            existing.IsConnected = true;
            room.NotifyChanged();
            return (room, existing, JoinRoomError.None);
        }

        if (room.IsFull)
        {
            return (null, null, JoinRoomError.RoomFull);
        }

        // Duplicate nicknames (e.g. matching the other player already in the room) are allowed.
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
        return (room, player, JoinRoomError.None);
    }

    private static JoinRoomError ValidateNickname(string nickname)
    {
        var trimmed = nickname?.Trim() ?? string.Empty;
        if (trimmed.Length < MinNicknameLength)
        {
            return JoinRoomError.NicknameTooShort;
        }

        if (trimmed.Length > MaxNicknameLength)
        {
            return JoinRoomError.NicknameTooLong;
        }

        return JoinRoomError.None;
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

    public void MarkConnected(string pin, Guid playerId)
    {
        if (_rooms.TryGetValue(pin, out var room))
        {
            var player = room.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null && !player.IsConnected)
            {
                player.IsConnected = true;
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

