using Godot;

public partial class PortalLink : Note
{
	public Area2D PortalNoteL;
	public Area2D PortalNoteR;
	public Line2D ConnectionLine;
	public float NotesHorizontalDistance;
	
	public override void _Ready()
	{
		this.PortalNoteL = GetNode<Area2D>("PortalNoteL");
		this.PortalNoteR = GetNode<Area2D>("PortalNoteR");
		this.ConnectionLine = GetNode<Line2D>("ConnectionLine");
		
		this.SpriteSize = GetSpriteSize(GetNode<Control>("PortalNoteL/NinePatchRect"));
	}
	
	/// <summary>
	/// Gets called when either the left or right note hit an Area2D
	/// </summary>
	/// <param name="hit">The hit collider</param>
	/// <param name="noteID">The noteID of the note that has entered the player's collider</param>
	public void OnAreaEntered(Area2D hit, int noteID)
	{
		PlayerController playerScript = hit as PlayerController;
		
		// Teleport player to other note
		float teleportXPos = noteID == 0 ? hit.GlobalPosition.X + this.NotesHorizontalDistance : hit.GlobalPosition.X - this.NotesHorizontalDistance;
		playerScript.GlobalPosition = playerScript.CalculatePlayerPosition(teleportXPos);

		// Get the correct note that has been hit
		Area2D note = noteID == 0 ? this.PortalNoteL : this.PortalNoteR;

		GameManager.instance.OnNoteHit(note.GlobalPosition, 2);
		GameManager.SFXManager.PlaySound("NoteSound");
		GameManager.NoteSpawner.PortalLinksQueue.Dequeue();
		QueueFree();
	}

	public override void _PhysicsProcess(double delta)
	{
		Fall(delta);
		
		if (!this._missed && DetectIfUnderPlayer(this.PortalNoteL.GlobalPosition.Y))
		{
			this._missed = true;
			GameManager.instance.OnNoteMiss();
		}
	}

	/// <summary>
	/// Called when the notes exit out of the screen at the bottom
	/// </summary>
	public override void OnScreenExited()
	{
		GameManager.NoteSpawner.PortalLinksQueue.Dequeue();
		QueueFree();
	}
	
	/// <summary>
	/// Checks if player is under the specified portalNote
	/// </summary>
	/// <param name="portalNote">The given portal note</param>
	/// <returns>Whether the player is under the given portal note</returns>
	public bool CheckIfPlayerIsUnderNote(Area2D portalNote)
	{
		PlayerController playerScript = GameManager.PlayerController;
		
		float playerHorizontalPosLeft = playerScript.GlobalPosition.X - playerScript.SpriteSize.X*0.5f;
		float playerHorizontalPosRight = playerScript.GlobalPosition.X + playerScript.SpriteSize.X*0.5f;
		
		float portalNoteHorizontalPosLeft = portalNote.GlobalPosition.X - this.SpriteSize.X*0.5f;
		float portalNoteHorizontalPosRight = portalNote.GlobalPosition.X + this.SpriteSize.X*0.5f;
		
		return playerHorizontalPosLeft < portalNoteHorizontalPosRight &&
			playerHorizontalPosRight > portalNoteHorizontalPosLeft;
	}
	
	/// <summary>
	/// Check if player is under one of the portalNotes 
	/// </summary>
	/// <returns>If the player is under a portal note, return the distance between the two portal notes, otherwise return 0.0f</returns>
	public float CheckIfPlayerIsUnder()
	{
		// Check if the player is under one of the portalNotes
		bool playerUnderLeft = CheckIfPlayerIsUnderNote(this.PortalNoteL);
		bool playerUnderRight = CheckIfPlayerIsUnderNote(this.PortalNoteR);
		
		if (playerUnderLeft || playerUnderRight)
		{
			// Get the distance correct distance for the highlightRect
			return playerUnderLeft ? this.NotesHorizontalDistance : -this.NotesHorizontalDistance;
		}
		return 0.0f;
	}
}
