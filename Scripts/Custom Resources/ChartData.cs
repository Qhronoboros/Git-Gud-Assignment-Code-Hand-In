using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ChartData : Resource
{
	[Export] public Array<NoteSpawnData> NotesSpawnData;
}
