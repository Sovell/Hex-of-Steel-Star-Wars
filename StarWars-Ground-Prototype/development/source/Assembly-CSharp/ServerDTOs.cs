using System;
using System.Collections.Generic;

public static class ServerDTOs
{
	public sealed class ProblemDetails
	{
		public string type;

		public string title;

		public int status;

		public string detail;

		public string instance;
	}

	public sealed class RegisterRequest
	{
		public string username;

		public string password;

		public string email;

		public string language;
	}

	public sealed class ResendMailConfirmationRequest
	{
		public string email;
	}

	public sealed class LoginRequest
	{
		public string username;

		public string password;
	}

	public sealed class LogoutRequest
	{
		public string refreshToken;
	}

	public sealed class RefreshRequest
	{
		public string token;
	}

	public sealed class LoginResponse
	{
		public Guid userId;

		public string username;

		public string accessToken;

		public DateTimeOffset expiresAtUtc;
	}

	public sealed class RefreshAccessResponse
	{
		public string accessToken;

		public DateTimeOffset expiresAtUtc;
	}

	public sealed class ConfirmEmailRequest
	{
		public string email;

		public string token;
	}

	public sealed class ChangeAccountLanguageRequest
	{
		public string language;
	}

	public sealed class ChangeBioRequest
	{
		public string bio;
	}

	public sealed class ChangeAvatarRequest
	{
		public byte[] avatarData;
	}

	public sealed class UserAccountResponse
	{
		public string username;

		public string email;

		public string language;

		public int gamesWon;

		public int gamesLost;

		public DateTimeOffset? lastLoginUtc;

		public DateTimeOffset createdAtUtc;
	}

	public sealed class DeleteAccountRequest
	{
		public string password;
	}

	public sealed class UserProfileResponse
	{
		public string username;

		public string language;

		public string bio;

		public int gamesWon;

		public int gamesLost;

		public int gamesForfeited;

		public int turnsPlayed;

		public DateTimeOffset? lastLoginUtc;

		public DateTimeOffset createdAtUtc;

		public bool banned;
	}

	public sealed class ForgotPasswordRequest
	{
		public string email;
	}

	public sealed class SetPasswordRequest
	{
		public string email;

		public string pinCode;

		public string newPassword;
	}

	public sealed class FetchGamesRequest
	{
		public GameSessionStates[] states;

		public string search;

		public bool iAmHost;

		public bool iAmIn;
	}

	public sealed class HostGameRequest
	{
		public string name;

		public string description;

		public string password;

		public PlayerSlotItem[] playerSlots;

		public List<string> mods;

		public string settings;

		public byte[] data;

		public string dataHash;
	}

	public sealed class JoinGameRequest
	{
		public Guid gameSessionId;

		public int slotIndex;

		public string password;
	}

	public sealed class LeaveGameRequest
	{
		public Guid gameSessionId;

		public int slotIndex;
	}

	public sealed class KickPlayerRequest
	{
		public Guid gameSessionId;

		public int slotIndex;
	}

	public sealed class EndTurnRequest
	{
		public Guid gameSessionId;

		public byte[] data;

		public string dataHash;

		public int turnNumber;
	}

	public sealed class EndGameRequest
	{
		public Guid gameSessionId;

		public Guid[] winnerUserIds;
	}

	public sealed class PlayerSlotItem
	{
		public int index;

		public string nation;

		public string username;

		public string playerName;

		public Guid? userId;

		public bool isPlayingFirst;

		public bool isAIcontrolled;
	}

	public sealed class GameSessionDetails
	{
		public Guid Id { get; set; }

		public GameSessionStates State { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public string HostUsername { get; set; }

		public string ActivePlayerUsername { get; set; }

		public int TurnNumber { get; set; }

		public bool IsPasswordProtected { get; set; }

		public string Settings { get; set; }

		public List<string> Mods { get; set; }

		public DateTimeOffset CreatedAtUtc { get; set; }

		public DateTimeOffset UpdatedAtUtc { get; set; }

		public DateTimeOffset? LastPlayedAtUtc { get; set; }

		public PlayerSlotItem[] PlayerSlots { get; set; }

		public bool IsHost => HostUsername == PlayerSettings.Instance.Username;

		public bool IsParticipant => Array.Exists(PlayerSlots, (PlayerSlotItem slot) => slot.username == PlayerSettings.Instance.Username);

		public bool IsActivePlayer => ActivePlayerUsername == PlayerSettings.Instance.Username;

		public int TotalSlotsCount => Array.FindAll(PlayerSlots, (PlayerSlotItem slot) => !slot.isAIcontrolled).Length;

		public int AvailableSlotsCount => Array.FindAll(PlayerSlots, (PlayerSlotItem slot) => !slot.isAIcontrolled && string.IsNullOrEmpty(slot.username)).Length;

		public int TakenSlotsCount => TotalSlotsCount - AvailableSlotsCount;

		public bool CanForfeit
		{
			get
			{
				if (State == GameSessionStates.OPEN)
				{
					return IsParticipant;
				}
				return false;
			}
		}

		public bool CanPlayTurn
		{
			get
			{
				if (State == GameSessionStates.OPEN)
				{
					return IsActivePlayer;
				}
				return false;
			}
		}

		public bool CanDeleteGame
		{
			get
			{
				if (State != GameSessionStates.FINISHED)
				{
					return IsHost;
				}
				return false;
			}
		}
	}
}
