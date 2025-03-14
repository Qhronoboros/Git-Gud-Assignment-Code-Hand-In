using Godot;

public partial class PlayerController : Area2D
{
	// HighlightRect script that is used for highlighting the area the player could teleport to using portalNotes
	public HighlightRect HighlightRect;
	
	// The blank space that's between the rectangle size and sprite of the player
	public Vector2 RectBlankSpace = new Vector2(30.0f, 30.0f);
	// Player spriteSize
	public Vector2 SpriteSize;
	
	// The horizontal speed of the player
	[Export] private float _speed = 500.0f;
	// The horizontal boostSpeed of the player
	[Export] private float _boostSpeed = 900.0f;
	private float _horizontalVel = 0.0f;
	
	public override void _Ready()
	{
		if (GameManager.PlayerController == null)
		{
			GameManager.PlayerController = this;
		}
		else 
		{
			GD.PrintErr($"A PlayerController already exists, deleting self");
			QueueFree();
		}
		
		// Get Highlight Rectangle
		this.HighlightRect = GetNode<HighlightRect>("HighlightRect");
		
		// Setup spriteSize for player
		this.SpriteSize = this.HighlightRect.Size - this.RectBlankSpace;
	}
	
	/// <summary>
	/// Get playerSpeed and set the player position
	/// </summary>
	/// <param name="delta"></param>
	public override void _PhysicsProcess(double delta)
	{
		float chosenSpeed = Input.IsActionPressed("Boost") ? this._boostSpeed : this._speed;
		
		float movementAxis = Input.GetAxis("Move Left", "Move Right");
		this._horizontalVel += movementAxis * chosenSpeed * (float)delta;
		
		this.GlobalPosition = CalculatePlayerPosition(this.GlobalPosition.X + this._horizontalVel);
		
		// Velocity fall off
		this._horizontalVel *= 0.5f;
	}
	
	/// <summary>
	/// Calculate the player's new position and keep the player inside the window by clamping it
	/// </summary>
	/// <param name="newPositionX">The player's new horizontal position</param>
	/// <returns>The player's new global position</returns>
	public Vector2 CalculatePlayerPosition(float newPositionX)
	{
		return new Vector2((float)Mathf.Clamp(newPositionX, 0.0f + this.SpriteSize.X*0.5f,
			GetViewport().GetVisibleRect().Size.X - this.SpriteSize.X*0.5f), this.GlobalPosition.Y);
	}
}
