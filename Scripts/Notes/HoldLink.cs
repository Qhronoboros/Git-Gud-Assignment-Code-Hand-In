using Godot;
using System;
// Maybe allow for more hold notes
public partial class HoldLink : Note
{
	public Area2D HoldNoteStart;
	public Area2D HoldNoteEnd;
	[Export] private PackedScene _connectionPolygonScene;
	public Polygon2D MainConnectionPolygon;
	public Line2D FeatherOutline;
	public Path2D PolygonPath;
	public PathFollow2D PathFollow;
	
	private float _polygonWidthAdjustment = -8;
	public float HalfWidth;
	private bool _playerIsOnPath = false;
	private bool _oneHoldNoteRemains = false;
	
	public float VerticalNoteDistance;
	public float PathStartLocalVerticalPos;
	public float PathEndLocalVerticalPos;
	
	private Callable _holdPulseCallable;
	
	public override void _Ready()
	{
		this.HoldNoteStart = GetNode<Area2D>("HoldNoteStart");
		this.HoldNoteEnd = GetNode<Area2D>("HoldNoteEnd");
		// Polygon shows the visual path for the player
		this.MainConnectionPolygon = GetNode<Polygon2D>("ConnectionPolygon");
		this.FeatherOutline = GetNode<Line2D>("ConnectionPolygon/FeatherOutline");
		// The actual path between the notes
		this.PolygonPath = GetNode<Path2D>("PolygonPath");
		this.PathFollow = GetNode<PathFollow2D>("PolygonPath/PathFollow2D");
		this.PathFollow.Visible = false;
		
		this.SpriteSize = GetSpriteSize(GetNode<Control>("HoldNoteStart/NinePatchRect"));
		
		this._holdPulseCallable = new Callable(this, "HoldPulse");
		
		this.HalfWidth = this.SpriteSize.X*0.5f + this._polygonWidthAdjustment;
	}

	public override void _PhysicsProcess(double delta)
	{
		Fall(delta);
		
		if (!this._missed && IsInstanceValid(this.HoldNoteStart) && DetectIfUnderPlayer(this.HoldNoteStart.GlobalPosition.Y))
		{
			this._missed = true;
			GameManager.instance.OnMiss();
		}
		
		PlayerController playerController = GameManager.PlayerController;
		Vector2 playerGlobalPos = playerController.GlobalPosition;
		
		// When player is between the start and end note
		if (this.PathStartLocalVerticalPos + GlobalPosition.Y > playerGlobalPos.Y &&
			this.PathEndLocalVerticalPos + GlobalPosition.Y < playerGlobalPos.Y)
		{
			// Connect timeout Signal to the HoldPulse function if not connected yet, also make pathFollow visible
			// if (!BPMTimer.IsConnected("timeout", holdPulseCallable)) { BPMTimer.Connect("timeout", holdPulseCallable); pathFollow.Visible = true; }
			
			// Check what position the player is between the two notes (ratio between 0.0 and 1.0)
			float playerPosYBetweenNotes = this.PathStartLocalVerticalPos + GlobalPosition.Y - playerGlobalPos.Y;
			this.PathFollow.ProgressRatio = playerPosYBetweenNotes / (this.VerticalNoteDistance - this.SpriteSize.Y);
			
			// Also check if player is close to the pathFollow horizontally
			if (Math.Abs(this.PathFollow.GlobalPosition.X - playerGlobalPos.X) - playerController.SpriteSize.X*0.5f < this.SpriteSize.X*0.5)
			{
				this._playerIsOnPath = true;
				
				// Get points of the mainPolygon
				Vector2[] points = this.MainConnectionPolygon.Polygon;
				
				// ? For some reason the polygon doesn't change/update when it's falling down too fast
				// ? It does get fixed when it's vertical position gets offset, absolutely no idea why it suddenly works when doing that
				float verticalOffset = GameManager.instance.NotespeedMultiplier * 3.5f;
				
				Vector2 pathFollowLeft = new Vector2(this.PathFollow.Position.X - this.HalfWidth, this.PathFollow.Position.Y - verticalOffset);
				Vector2 pathFollowRight = new Vector2(this.PathFollow.Position.X + this.HalfWidth, this.PathFollow.Position.Y - verticalOffset);
				
				// Check if player is above the bottom part of the polygon, if yes instantiate a severed polygon replacing the bottom part
				if (playerController.GlobalPosition.Y + playerController.SpriteSize.Y*0.5f < this.MainConnectionPolygon.GlobalPosition.Y + points[3].Y)
				{
					Polygon2D severedPolygon = this._connectionPolygonScene.Instantiate<Polygon2D>();
					AddChild(severedPolygon);
					
					// Clone the points of the mainPolygon to the serveredPolygon and
					// move the top points of the severedPolygon towards the sides of pathFollow
					Vector2[] severedPoints = (Vector2[])points.Clone();
					severedPoints[0] = pathFollowLeft;
					severedPoints[1] = pathFollowRight;
					
					severedPolygon.GetNode<Line2D>("FeatherOutline").Points = severedPolygon.Polygon = severedPoints;
				}
				
				// Move the bottom points of the mainPolygon towards the sides of pathFollow
				points[2] = pathFollowRight;
				points[3] = pathFollowLeft;
				
				// Reassign points for the visible polygon
				this.MainConnectionPolygon.Polygon = this.FeatherOutline.Points = points;
			}
			else { this._playerIsOnPath = false; }
		}	// Disconnect timeout Signal to the HoldPulse function if connected, also make pathFollow invisible
		// else if (BPMTimer.IsConnected("timeout", holdPulseCallable)) { BPMTimer.Disconnect("timeout", holdPulseCallable); pathFollow.Visible = false;}
	}

	/// <summary>
	/// Gets called whenever the player is within the pathFollow and the BPM Timer triggers
	/// </summary>
	public void HoldPulse()
	{
		// Add or Remove a point depending if the player is on the path
		if (this._playerIsOnPath)
		{
			GameManager.instance.OnHit(1);
		}
		else
		{
			GameManager.instance.OnMiss();
		}
	}
	
	/// <summary>
	/// Called when either note enter the player
	/// </summary>
	/// <param name="hit">The hit collider</param>
	/// <param name="noteID">The noteID of the note that has entered the player's collider</param>
	public void _OnAreaEntered(Area2D hit, int noteID)
	{
		GameManager.instance.OnHit(1);
		GameManager.SFXManager.PlaySound("NoteSound");
		
		if (noteID == 0)
		{
			this.HoldNoteStart.QueueFree();
		}
		else
		{
			this.HoldNoteEnd.QueueFree();
		}
		
		CheckIfLastHoldNote();
	}

	/// <summary>
	/// Called when either note exit out of the screen at the bottom
	/// </summary>
	/// <param name="noteID">The noteID of the note that has exited the screen</param>
	public void _OnScreenExited(int noteID)
	{
		if (noteID == 0)
		{
			this.HoldNoteStart.QueueFree();
		}
		else
		{
			this.HoldNoteEnd.QueueFree();
		}
		
		CheckIfLastHoldNote();
	}
	
	/// <summary>
	/// Destroy itself whenever there aren't any hold notes active anymore
	/// </summary>
	public void CheckIfLastHoldNote()
	{
		if (this._oneHoldNoteRemains)
		{
			QueueFree();
		}
		else
		{
			this._oneHoldNoteRemains = true;
		}
	}
}
