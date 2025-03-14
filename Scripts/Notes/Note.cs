using Godot;

public partial class Note : Node2D
{
	public static float FallSpeed = 300.0f;
	public Vector2 SpriteSize;
	protected Vector2 _rectBlankSpace = new Vector2(30.0f, 30.0f);
	protected bool _missed = false;
	
	public override void _Ready()
	{
		this.SpriteSize = GetSpriteSize(GetNode<Control>("NinePatchRect"));
	}
	
	public override void _PhysicsProcess(double delta)
	{
		Fall(delta);
		
		if (!this._missed && DetectIfUnderPlayer(GlobalPosition.Y))
		{
			this._missed = true;
			GameManager.instance.OnMiss();
		}
	}

	/// <summary>
	/// Moves the note down
	/// </summary>
	/// <param name="delta"></param>
	public void Fall(double delta)
	{
		this.Position = new Vector2(this.Position.X, this.Position.Y + FallSpeed * GameManager.instance.NotespeedMultiplier * (float)delta);
	}

	/// <summary>
	/// Called when note enters the player
	/// </summary>
	/// <param name="hit">The hit collider</param>
	public void _OnAreaEntered(Area2D hit)
	{
		GameManager.SFXManager.PlaySound("NoteSound");
		GameManager.instance.OnHit(1);
		QueueFree();
	}
	
	/// <summary>
	/// Called when note exits out of the screen at the bottom
	/// </summary>
	public virtual void _OnScreenExited()
	{
		QueueFree();
	}
	
	/// <summary>
	/// Get Width and Height of NinePatchRectangle in pixels
	/// </summary>
	/// <param name="rect">The NinePatchRect from the note</param>
	/// <returns></returns>
	public Vector2 GetSpriteSize(Control rect)
	{
		return rect.Size - this._rectBlankSpace;
		// return new Vector2(sprite.Texture.GetWidth() * sprite.Scale.X, sprite.Texture.GetHeight() * sprite.Scale.Y);
	}
	
	/// <summary>
	/// Checks if the note is under the player
	/// </summary>
	/// <param name="noteVerticalPosition">The vertical position of the note</param>
	/// <returns>Whether the note is under the player</returns>
	public bool DetectIfUnderPlayer(float noteVerticalPosition)
	{
		PlayerController playerController = GameManager.PlayerController;
		return noteVerticalPosition - this.SpriteSize.Y > playerController.GlobalPosition.Y + playerController.SpriteSize.Y*0.5f;
	}
}
