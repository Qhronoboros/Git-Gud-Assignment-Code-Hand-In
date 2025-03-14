using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class NoteSpawner : Node
{
	[Export] private ChartData _chartData;
	private Array<NoteSpawnData> _notesSpawnData;
	private Timer _spawnTimer;
	private Path2D _spawnPath;
	private PathFollow2D _spawnLocation;
	[Export] private PackedScene _noteScene;
	[Export] private PackedScene _portalLinkScene;
	[Export] private PackedScene _holdLinkScene;
	
	public Queue<PortalLink> PortalLinksQueue = new Queue<PortalLink>();
	
	public override void _Ready()
	{
		if (GameManager.NoteSpawner == null)
		{
			GameManager.NoteSpawner = this;
		}
		else 
		{
			GD.PrintErr($"A NoteSpawner already exists, deleting self");
			QueueFree();
		}
		
		this._spawnTimer = GetNode<Timer>("SpawnTimer");
		this._spawnPath = GetNode<Path2D>("SpawnPath");
		this._spawnLocation = GetNode<PathFollow2D>("SpawnPath/SpawnLocation");
		
		this._notesSpawnData = _chartData.NotesSpawnData.Duplicate(true);
		
		// _spawnTimer.Start();
	}

	/// <summary>
	/// Gets called by the event OnNewBeat and spawns a note by getting values from NotesSpawnData
	/// </summary>
	/// <param name="currentBeat">The current beat of the song</param>
	public void SpawnNotesOnBeat(float currentBeat)
	{
		for (int i = 0; i < this._notesSpawnData.Count; i++)
		{
			NoteSpawnData noteSpawnData = this._notesSpawnData[i];
			
			// Return if given note's beat position is not before the next beat
			if (noteSpawnData.Beat >= currentBeat + 1)
				return;
			
			// Remove the note from the array
			this._notesSpawnData.RemoveAt(i);
			i--;
			
			// Return if the given note's beat position is not between the currentBeat and the next beat
			if (Mathf.Floor(noteSpawnData.Beat) != currentBeat)
				return;
			
			// Spawn a note, the type of note depends on if the note contains a second position and second beat
			if (noteSpawnData.SecondHorizontalPosition == 0.0f)
			{
				SpawnNormalNote(noteSpawnData, currentBeat);
			}
			else if (noteSpawnData.SecondBeat == 0.0f) 
			{
				SpawnPortalNote(noteSpawnData, currentBeat);
			}
			else
			{
				SpawnHoldNote(noteSpawnData, currentBeat);
			}
		}
	}

	/// <summary>
	/// Calculate the vertical position of the noteBeat if the noteBeat != currentBeat (the noteBeat is a decimal number)
	/// </summary>
	/// <param name="currentBeat">The current beat of the song</param>
	/// <param name="noteBeat">The beat where the note should hit the judgementLine</param>
	/// <returns>The vertical position of the note</returns>
	private float CalculateVerticalPositionOffset(float currentBeat, float noteBeat)
	{
		return -(float)((noteBeat - currentBeat) * GameManager.Conductor.SecondsPerBeat * GameManager.Conductor.AmplifiedNoteSpeed);
	}
	
	/// <summary>
	/// Spawns a normalNote
	/// </summary>
	/// <param name="noteSpawnData">Data that consists of positions and beats</param>
	/// <param name="currentBeat">The current beat of the song</param>
	private void SpawnNormalNote(NoteSpawnData noteSpawnData, float currentBeat)
	{
		Note noteObject = this._noteScene.Instantiate<Note>();
		AddChild(noteObject);
		
		Vector2 spriteSize = noteObject.SpriteSize;
		float verticalPos = CalculateVerticalPositionOffset(currentBeat, noteSpawnData.Beat);
		noteObject.GlobalPosition = new Vector2(noteSpawnData.HorizontalPosition * GetViewport().GetVisibleRect().Size.X, verticalPos - spriteSize.Y);
	}

	/// <summary>
	/// Spawns a portalNote, which consists of two separate notes
	/// </summary>
	/// <param name="noteSpawnData">Data that consists of positions and beats</param>
	/// <param name="currentBeat">The current beat of the song</param>
	private void SpawnPortalNote(NoteSpawnData noteSpawnData, float currentBeat)
	{
		PortalLink portalLinkObject = this._portalLinkScene.Instantiate<PortalLink>();
		AddChild(portalLinkObject);
		
		Area2D portalNoteL = portalLinkObject.PortalNoteL;
		Area2D portalNoteR = portalLinkObject.PortalNoteR;
		
		Vector2 spriteSize = portalLinkObject.SpriteSize;
		
		(float leftPosition, float rightPosition) = noteSpawnData.HorizontalPosition < noteSpawnData.SecondHorizontalPosition ?
			(noteSpawnData.HorizontalPosition, noteSpawnData.SecondHorizontalPosition) : 
			(noteSpawnData.SecondHorizontalPosition, noteSpawnData.HorizontalPosition);
		
		float verticalPos = CalculateVerticalPositionOffset(currentBeat, noteSpawnData.Beat);
		portalNoteL.GlobalPosition = new Vector2(leftPosition * GetViewport().GetVisibleRect().Size.X, verticalPos - spriteSize.Y);
		portalNoteR.GlobalPosition = new Vector2(rightPosition * GetViewport().GetVisibleRect().Size.X, verticalPos - spriteSize.Y);

		// horizontal distance between the two notes
		portalLinkObject.NotesHorizontalDistance = portalNoteR.Position.X - portalNoteL.Position.X;

		// Set up connection line between notes
		Line2D connectionLine = portalLinkObject.ConnectionLine;
		connectionLine.ClearPoints();
		connectionLine.AddPoint(new Vector2(portalNoteL.Position.X + spriteSize.X*0.5f, verticalPos - spriteSize.Y));
		connectionLine.AddPoint(new Vector2(portalNoteR.Position.X - spriteSize.X*0.5f, verticalPos - spriteSize.Y));
		
		// Add portalLink to PortalLinksQueue
		PortalLinksQueue.Enqueue(portalLinkObject);
	}
	
	/// <summary>
	/// Spawns a holdNote, which consists of two separate notes that are connected with a line the player needs to follow
	/// </summary>
	/// <param name="noteSpawnData">Data that consists of positions and beats</param>
	/// <param name="currentBeat">The current beat of the song</param>
	private void SpawnHoldNote(NoteSpawnData noteSpawnData, float currentBeat)
	{
		HoldLink holdLinkObject = this._holdLinkScene.Instantiate<HoldLink>();
		AddChild(holdLinkObject);

		Area2D holdNoteStart = holdLinkObject.HoldNoteStart;
		Area2D holdNoteEnd = holdLinkObject.HoldNoteEnd;
		
		Vector2 spriteSize = holdLinkObject.SpriteSize;
		
		float firstVerticalPos = CalculateVerticalPositionOffset(currentBeat, noteSpawnData.Beat);
		float secondVerticalPos = CalculateVerticalPositionOffset(currentBeat, noteSpawnData.SecondBeat);
		
		holdLinkObject.VerticalNoteDistance = MathF.Abs(secondVerticalPos - firstVerticalPos);
		
		holdNoteStart.GlobalPosition = new Vector2(noteSpawnData.HorizontalPosition * GetViewport().GetVisibleRect().Size.X, firstVerticalPos - spriteSize.Y);
		holdNoteEnd.GlobalPosition = new Vector2(noteSpawnData.SecondHorizontalPosition * GetViewport().GetVisibleRect().Size.X, secondVerticalPos - spriteSize.Y);
		
		holdLinkObject.PathStartLocalVerticalPos = holdNoteStart.Position.Y - spriteSize.Y*0.5f;
		holdLinkObject.PathEndLocalVerticalPos = holdNoteEnd.Position.Y + spriteSize.Y*0.5f;
		
		float halfWidth = holdLinkObject.HalfWidth;
		// Set the polygon for the visual hold line between the two hold notes
		holdLinkObject.MainConnectionPolygon.Polygon = holdLinkObject.FeatherOutline.Points = new Vector2[]
		{
			new Vector2(holdNoteEnd.Position.X - halfWidth, holdLinkObject.PathEndLocalVerticalPos),
			new Vector2(holdNoteEnd.Position.X + halfWidth, holdLinkObject.PathEndLocalVerticalPos),
			new Vector2(holdNoteStart.Position.X + halfWidth, holdLinkObject.PathStartLocalVerticalPos),
			new Vector2(holdNoteStart.Position.X - halfWidth, holdLinkObject.PathStartLocalVerticalPos)
		};
		
		Curve2D curve = new Curve2D();
		curve.AddPoint(new Vector2(holdNoteStart.Position.X, holdLinkObject.PathStartLocalVerticalPos));
		curve.AddPoint(new Vector2(holdNoteEnd.Position.X, holdLinkObject.PathEndLocalVerticalPos));

		Sprite2D sprite = holdLinkObject.PathFollow.GetNode<Sprite2D>("Sprite2D");
		sprite.Position = new Vector2(GameManager.PlayerController.SpriteSize.Y*0.5f, sprite.Position.Y);

		holdLinkObject.PolygonPath.Curve = curve;
	}
	
	/// <summary>
	/// Spawns a random note (Is not getting used)
	/// </summary>
	private void OnTimerTimeout()
	{
		// Randomize note type
		// GD.Randi() % 3
		switch (GD.Randi() % 3)
		{
			case 0:
				Note noteObject = this._noteScene.Instantiate<Note>();
				AddChild(noteObject);
				
				Vector2 spriteSize = noteObject.SpriteSize;
				noteObject.Position = GetLocationOnCurve(spriteSize.X*0.5f, spriteSize.X*0.5f, spriteSize.Y);
				
				break;
			case 1:
				PortalLink portalLinkObject = this._portalLinkScene.Instantiate<PortalLink>();
				AddChild(portalLinkObject);
				
				Area2D portalNoteL = portalLinkObject.PortalNoteL;
				Area2D portalNoteR = portalLinkObject.PortalNoteR;
				
				spriteSize = portalLinkObject.SpriteSize;
				
				portalNoteL.Position = GetLocationOnCurve(spriteSize.X*0.5f, spriteSize.X*1.5f, spriteSize.Y);
				portalNoteR.Position = GetLocationOnCurve(portalNoteL.Position.X + spriteSize.X, spriteSize.X*0.5f, spriteSize.Y);

				// horizontal distance between the two notes
				portalLinkObject.NotesHorizontalDistance = portalNoteR.Position.X - portalNoteL.Position.X;

				// Set up connection line between notes
				Line2D connectionLine = portalLinkObject.ConnectionLine;
				connectionLine.ClearPoints();
				connectionLine.AddPoint(new Vector2(portalNoteL.Position.X + spriteSize.X*0.5f, -spriteSize.Y));
				connectionLine.AddPoint(new Vector2(portalNoteR.Position.X - spriteSize.X*0.5f, -spriteSize.Y));
				
				// Add portalLink to PortalLinksQueue
				PortalLinksQueue.Enqueue(portalLinkObject);
				
				break;
			case 2:
				HoldLink holdLinkObject = this._holdLinkScene.Instantiate<HoldLink>();
				AddChild(holdLinkObject);

				Area2D holdNoteStart = holdLinkObject.HoldNoteStart;
				Area2D holdNoteEnd = holdLinkObject.HoldNoteEnd;
				
				spriteSize = holdLinkObject.SpriteSize;
				
				holdLinkObject.VerticalNoteDistance = 400.0f;
				holdNoteStart.Position = GetLocationOnCurve(spriteSize.X*0.5f, spriteSize.X*0.5f, spriteSize.Y);
				holdNoteEnd.Position = GetLocationOnCurve(spriteSize.X*0.5f, spriteSize.X*0.5f, spriteSize.Y + holdLinkObject.VerticalNoteDistance);
				
				holdLinkObject.PathStartLocalVerticalPos = holdNoteStart.Position.Y - spriteSize.Y*0.5f;
				holdLinkObject.PathEndLocalVerticalPos = holdNoteEnd.Position.Y + spriteSize.Y*0.5f;
				
				float halfWidth = holdLinkObject.HalfWidth;
				// Set the polygon for the visual hold line between the two hold notes
				holdLinkObject.MainConnectionPolygon.Polygon = holdLinkObject.FeatherOutline.Points = new Vector2[]
				{
					new Vector2(holdNoteEnd.Position.X - halfWidth, holdLinkObject.PathEndLocalVerticalPos),
					new Vector2(holdNoteEnd.Position.X + halfWidth, holdLinkObject.PathEndLocalVerticalPos),
					new Vector2(holdNoteStart.Position.X + halfWidth, holdLinkObject.PathStartLocalVerticalPos),
					new Vector2(holdNoteStart.Position.X - halfWidth, holdLinkObject.PathStartLocalVerticalPos)
				};
				
				Curve2D curve = new Curve2D();
				curve.AddPoint(new Vector2(holdNoteStart.Position.X, holdLinkObject.PathStartLocalVerticalPos));
				curve.AddPoint(new Vector2(holdNoteEnd.Position.X, holdLinkObject.PathEndLocalVerticalPos));

				Sprite2D sprite = holdLinkObject.PathFollow.GetNode<Sprite2D>("Sprite2D");
				sprite.Position = new Vector2(GameManager.PlayerController.SpriteSize.Y*0.5f, sprite.Position.Y);

				holdLinkObject.PolygonPath.Curve = curve;

				break;
		}
	}
	
	/// <summary>
	/// Returns a location on a horizontal Curve2D at the top of the screen with the given offsets and height
	/// </summary>
	/// <param name="leftOffset">The offset on the left side</param>
	/// <param name="rightOffset">The offset on the right side</param>
	/// <param name="height">The vertical offset</param>
	/// <returns></returns>
	private Vector2 GetLocationOnCurve(float leftOffset, float rightOffset, float height)
	{
		Curve2D curve = new Curve2D();
		// Create start and endpoint of curve
		curve.AddPoint(new Vector2(leftOffset, -height));
		curve.AddPoint(new Vector2(GetViewport().GetVisibleRect().Size.X - rightOffset, -height));
		_spawnPath.Curve = curve;
		
		// Get random location on the curve
		_spawnLocation.ProgressRatio = GD.Randf();
		return _spawnLocation.Position;
	}
	
}
