using Godot;
using Godot.Collections;

[GlobalClass]
public partial class HighscoreData : Resource
{
	[Export] public Array<int> Highscores = new Array<int>();
	
	public void AddScore(int score)
	{
		Highscores.Add(score);
		
		// Sorts the array in ascending order
		Highscores.Sort();
	}
}
