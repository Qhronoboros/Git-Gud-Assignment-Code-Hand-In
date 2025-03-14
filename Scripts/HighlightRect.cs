using Godot;
using System.Collections.Generic;

public partial class HighlightRect : NinePatchRect
{
	public override void _PhysicsProcess(double delta)
	{
		if (GameManager.PlayerController != null && GameManager.NoteSpawner != null)
			this.Visible = PortalLinkHighlight();
	}
	
	/// <summary>
	/// Highlights the area the player can teleport to when under a portal note
	/// </summary>
	/// <returns>Whether the player is under a portalNote</returns>
	private bool PortalLinkHighlight()
	{
		Queue<PortalLink> PortalLinksQueue = GameManager.NoteSpawner.PortalLinksQueue;
	
		if (PortalLinksQueue.Count == 0)
			return false;
		
		// Get the first portalLink in the queue
		PortalLink currentPortalLink = PortalLinksQueue.Peek();
		
		// Get the distance between the notes if the player is under one of the portalNotes
		float addDistance = currentPortalLink.CheckIfPlayerIsUnder();

		// If player is not under a portalNote
		if (addDistance == 0.0f) 
			return false;

		// Highlight the teleport position
		this.GlobalPosition = GetHighlightTeleportPosition(addDistance);
		return true;
	}
	
	/// <summary>
	/// Get the Highlight position where the player could teleport to
	/// </summary>
	/// <param name="addDistance">The distance between the player and the highlighted area</param>
	/// <returns>The highlight position</returns>
	public Vector2 GetHighlightTeleportPosition(float addDistance)
	{
		PlayerController player = GameManager.PlayerController;
		return player.CalculatePlayerPosition(player.GlobalPosition.X + addDistance) - this.Size * 0.5f;
	}
}
