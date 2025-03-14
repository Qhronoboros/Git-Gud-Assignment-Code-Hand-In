using Godot;

[GlobalClass]
public partial class NoteSpawnData : Resource
{
	// The beat the note will be on
	[Export] public float Beat;
	// The horizontal position the note will spawn (from 0.0 to 1.0)
	[Export] public float HorizontalPosition;
	
	// If it's a hold note, use second beat and second horizontal position for the second hold note that comes after
	// If it's a portal note, use the second horizontal position for the second portal note
	[Export] public float SecondBeat;
	[Export] public float SecondHorizontalPosition;
}
