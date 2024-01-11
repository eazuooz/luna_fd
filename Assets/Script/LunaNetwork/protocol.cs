using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunaChatServer
{
	public enum PROTOCOL : short
	{
		BEGIN = 0,

		CHAT_MSG_REQ = 1,
		CHAT_MSG_ACK = 2,

		END
	}
}

namespace LunaGameServer
{
	public enum PROTOCOL : short
	{
		BEGIN = 0,

		// �ε��� �����ض�.
		START_LOADING = 1,

		LOADING_COMPLETED = 2,

		// ���� ����.
		GAME_START = 3,

		// �� ����.
		START_PLAYER_TURN = 4,

		// Ŭ���̾�Ʈ�� �̵� ��û.
		MOVING_REQ = 5,

		// �÷��̾ �̵� ������ �˸���.
		PLAYER_MOVED = 6,

		// Ŭ���̾�Ʈ�� �� ������ �������� �˸���.
		TURN_FINISHED_REQ = 7,

		// ���� �÷��̾ ���� ���� �����Ǿ���.
		ROOM_REMOVED = 8,

		// ���ӹ� ���� ��û.
		ENTER_GAME_ROOM_REQ = 9,

		// ���� ����.
		GAME_OVER = 10,

		END
	}
}
