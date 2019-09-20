﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomViewer))]
public class PlanetGenerator : MonoBehaviour
{
	private PlanetData planetData;
	private Room startRoom;
	private RoomViewer viewer;
	private Room activeRoom;

	public int minBranchLength, maxBranchLength;
	public int minBranchCount;
	private const int MAX_BRANCH_COUNT = 4;
	public int minDeadEndCount, maxDeadEndCount;

	public delegate void GenerationCompleteEventHandler();
	public event GenerationCompleteEventHandler OnGenerationComplete;
	public delegate void RoomChangedEventHandler(Room newRoom, Direction direction);
	public event RoomChangedEventHandler OnRoomChanged;

	public float randomEmptyRoomWeighting,
		randomPuzzleRoomWeighting,
		randomEnemiesRoomWeighting,
		randomTreasureRoomWeighting,
		randomNpcRoomWeighting;

	public PuzzleTypeWeightings puzzleRoomWeightings;

	private void Start()
	{
		viewer = GetComponent<RoomViewer>();
		Generate();
		//LoadCurrentRoom();
		viewer.ShowAllRooms(planetData);
	}

	private void Generate()
	{
		planetData = new PlanetData();
		
		Room currentRoom = null;
		int keyLevel = 0;

		//rule 1 -	Create starter room
		startRoom = planetData.AddRoom(RoomType.Start, new Vector2Int(0, 0), puzzleRoomWeightings, null);

		//rule 2 -	"Current Room" is starter room
		currentRoom = startRoom;

		//rule 7 -	Repeat steps 3 - 6 Y amount of times.
		int branchCount = Mathf.Max(1, Random.Range(minBranchCount, MAX_BRANCH_COUNT));
		for (int j = 0; j < branchCount; j++)
		{
			//rule 3 -	Create a single branch from the "Current room" until X rooms have been
			//			created. Branch can't overlap with existing rooms. If branch meets a dead
			//			end, end the branch.
			int branchLength = Mathf.Max(1,
				Random.Range(minBranchLength, maxBranchLength));
			for (int i = 0; i < branchLength; i++)
			{
				Room newRoom = CreateRandomExit(planetData, currentRoom);
				if (newRoom == currentRoom) break;
				currentRoom = newRoom;
			}

			//rule 4 -	Place a key at the end of that branch
			currentRoom.AddKey((RoomKey.KeyColour)keyLevel);

			//rule 5 -	Create a locked exit to a new room from any existing room except the end of
			//			that branch.
			List<Room> existingRooms = planetData.GetRooms();
			Room lockRoom = currentRoom;
			do
			{
				int randomIndex = Random.Range(0, existingRooms.Count);
				lockRoom = existingRooms[randomIndex];
			} while (lockRoom == currentRoom || lockRoom.ExitCount() == 4);
			lockRoom = CreateRandomExit(planetData, lockRoom, true, (RoomKey.KeyColour)keyLevel,
				j == branchCount - 1);
			keyLevel++;

			//rule 6 -	"Current room" is the new room on the other side of the locked exit
			currentRoom = lockRoom;
		}

		//rule 8 -	"Current room" is the final room
		planetData.finalRoom = currentRoom;

		//rule 9 -	Create "Dead end" branches Z times of X length from any room except the boss
		//			room.
		branchCount = Mathf.Max(0, Random.Range(minDeadEndCount, maxDeadEndCount));
		for (int i = 0; i < branchCount; i++)
		{
			List<Room> existingRooms = planetData.GetRooms();
			Room deadEndStart = null;
			do
			{
				int randomIndex = Random.Range(0, existingRooms.Count);
				deadEndStart = existingRooms[randomIndex];
			} while (deadEndStart == planetData.finalRoom || deadEndStart.ExitCount() == 4);
			currentRoom = deadEndStart;

			int branchLength = Mathf.Max(1,
				Random.Range(minBranchLength, maxBranchLength));
			for (int j = 0; j < branchLength; j++)
			{
				Room newRoom = CreateRandomExit(planetData, currentRoom);
				if (newRoom == currentRoom) break;
				currentRoom = newRoom;
			}
		}

		for (int i = 0; i < planetData.GetRoomCount(); i++)
		{
			planetData.GetRooms()[i].GenerateContent();
		}

		for (int i = 0; i < planetData.GetRoomCount(); i++)
		{
			planetData.GetRooms()[i].GenerateOuterWalls();
		}

		activeRoom = startRoom;
		OnGenerationComplete?.Invoke();
	}

	private Room CreateRandomExit(PlanetData data, Room room, bool locked = false,
		RoomKey.KeyColour colour = RoomKey.KeyColour.Blue, bool bossRoom = false)
	{
		if (room.ExitCount() == 4) return room;

		List<Direction> directions = new List<Direction>
			{
				Direction.Up, Direction.Right, Direction.Down, Direction.Left
			};
		//remove any directions that are not available
		for (int j = directions.Count - 1; j >= 0; j--)
		{
			Vector2Int roomPos = AddDirection(room.position, directions[j]);
			if (data.GetRoomAtPosition(roomPos) != null)
			{
				directions.RemoveAt(j);
			}
		}

		//if no available directions then we're in a dead end
		if (directions.Count == 0) return room;

		Direction randomDirection = directions[Random.Range(0, directions.Count)];
		Vector2Int pos = AddDirection(room.position, randomDirection);
		RoomType type = bossRoom ? RoomType.Boss : PickRandomRoomType();
		Room newRoom = data.AddRoom(type, pos, puzzleRoomWeightings, room);
		if (locked)
		{
			ConnectWithLock(room, newRoom, randomDirection, colour);
		}
		else
		{
			Connect(room, newRoom, randomDirection);
		}
		return newRoom;
	}

	private RoomType PickRandomRoomType()
	{
		float totalWeighting =
			randomEmptyRoomWeighting +
			randomPuzzleRoomWeighting +
			randomEnemiesRoomWeighting +
			randomTreasureRoomWeighting +
			randomNpcRoomWeighting;
		float randomValue = Random.Range(0f, totalWeighting);

		if ((randomValue = randomValue - randomEmptyRoomWeighting) < 0f)
		{
			return RoomType.Empty;
		}
		if ((randomValue = randomValue - randomPuzzleRoomWeighting) < 0f)
		{
			return RoomType.Puzzle;
		}
		if ((randomValue = randomValue - randomEnemiesRoomWeighting) < 0f)
		{
			return RoomType.Enemies;
		}
		if ((randomValue = randomValue - randomTreasureRoomWeighting) < 0f)
		{
			return RoomType.Treasure;
		}
		return RoomType.NPC;
	}

	private void Connect(Room a, Room b, Direction dir)
	{
		switch (dir)
		{
			case Direction.Up:
				ConnectVertically(a, b);
				break;
			case Direction.Right:
				ConnectHorizontally(a, b);
				break;
			case Direction.Down:
				ConnectVertically(b, a);
				break;
			case Direction.Left:
				ConnectHorizontally(b, a);
				break;
		}
	}

	private void ConnectWithLock(Room a, Room b, Direction dir, RoomKey.KeyColour colour)
	{
		switch (dir)
		{
			case Direction.Up:
				LockVertically(a, b, colour);
				break;
			case Direction.Right:
				LockHorizontally(a, b, colour);
				break;
			case Direction.Down:
				LockVertically(b, a, colour);
				break;
			case Direction.Left:
				LockHorizontally(b, a, colour);
				break;
		}
	}

	private void ConnectVertically(Room lower, Room upper)
	{
		int exitXPos = Random.Range(3, lower.GetWidth() - 3);
		lower.AddUpExit(exitXPos);
		lower.SetRoom(upper, Direction.Up);
		upper.AddDownExit(exitXPos);
		upper.SetRoom(lower, Direction.Down);
	}

	private void LockVertically(Room lower, Room upper, RoomKey.KeyColour lockColour)
	{
		ConnectVertically(lower, upper);
		lower.Lock(Direction.Up, lockColour);
		upper.Lock(Direction.Down, lockColour);
	}

	private void ConnectHorizontally(Room left, Room right)
	{
		int exitYPos = Random.Range(3, left.GetHeight() - 3);
		left.AddRightExit(exitYPos);
		left.SetRoom(right, Direction.Right);
		right.AddLeftExit(exitYPos);
		right.SetRoom(left, Direction.Left);
	}

	private void LockHorizontally(Room left, Room right, RoomKey.KeyColour lockColour)
	{
		ConnectHorizontally(left, right);
		left.Lock(Direction.Right, lockColour);
		right.Lock(Direction.Left, lockColour);
	}

	private Vector2Int AddDirection(Vector2Int v, Direction dir)
	{
		switch (dir)
		{
			case Direction.Up:
				v.y++;
				break;
			case Direction.Right:
				v.x++;
				break;
			case Direction.Down:
				v.y--;
				break;
			case Direction.Left:
				v.x--;
				break;
		}
		return v;
	}

	public bool Go(Direction direction)
	{
		activeRoom = activeRoom.GetRoom(direction) ?? activeRoom;
		LoadCurrentRoom();
		OnRoomChanged?.Invoke(activeRoom, direction);
		return true;
	}

	private void LoadCurrentRoom()
	{
		viewer.ShowRoom(planetData, activeRoom,
			activeRoom.position * activeRoom.GetDimensions());
	}

	private Room GetRoomAtPosition(int x, int y) => GetRoomAtPosition(new Vector2Int(x, y));

	private Room GetRoomAtPosition(Vector2Int position) => planetData.GetRoomAtPosition(position);

	private bool RoomExists(Vector2Int position) => GetRoomAtPosition(position) != null;

	public Room GetActiveRoom() => activeRoom;
}
