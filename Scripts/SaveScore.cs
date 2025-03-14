using Godot;

public partial class SaveScore : Node
{
	const string savePath = "user://save/";
	const string fileName = "Highscores.tres";
	
	public HighscoreData HighscoreData = new HighscoreData();
	
	public override void _Ready()
	{
		GameManager.SaveScore = this;
		
		// Create the path/folder if there isn't one yet
		DirAccess.MakeDirAbsolute(savePath);
		
		// If the highscore save exists, load it, otherwise create an empty resource file.
		if (ResourceLoader.Exists(savePath + fileName))
		{
			LoadData();
		}
		else
		{
			SaveData();
		}
	}

	/// <summary>
	/// Loads the Resource data
	/// </summary>
	public void LoadData()
	{
		HighscoreData = ResourceLoader.Load(savePath + fileName) as HighscoreData;
		
		for (int i = HighscoreData.Highscores.Count - 1; i >= 0; i--)
		{
			GD.Print(HighscoreData.Highscores[i]);
		}
	}
	
	/// <summary>
	/// Saves the Resource data
	/// </summary>
	public void SaveData()
	{
		ResourceSaver.Save(HighscoreData, savePath + fileName);
	}
	
	/// <summary>
	/// Add the current score to the Resource and save it
	/// </summary>
	public void SaveCurrentScore()
	{
		HighscoreData.AddScore(GameManager.instance.Score);
		SaveData();
	}
	
	/// <summary>
	/// Checks if the given score is a highscore
	/// </summary>
	/// <param name="score">The player's score</param>
	/// <returns>Whether the score is a highscore</returns>
	public bool IsHighscore(int score)
	{
		// If there is no highscore yet, return true, otherwise if score is higher than highest score in array, return true
		return HighscoreData.Highscores.Count == 0 || score > HighscoreData.Highscores[HighscoreData.Highscores.Count - 1];
	}
}
