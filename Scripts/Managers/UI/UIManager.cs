using Godot;
using Godot.Collections;

public partial class UIManager : Node
{
	private Label _scoreLabel;
	private Label _comboLabel;
	private Label _multiplierLabel;
	
	private GameplayHUD _gameplayHUD;
	private ScoreScreen _scoreScreen;
	
	private RichTextLabel _highscoresLabel;
	
	public override void _Ready()
	{
		if (GameManager.UIManager == null)
		{
			GameManager.UIManager = this;
		}
		else 
		{
			GD.PrintErr($"A UIManager already exists, deleting self");
			QueueFree();
		}
		
		switch (GetTree().CurrentScene.Name)
		{
			case "MainMenu":	// Get highscores for display in the main menu
				this._highscoresLabel = GetNodeOrNull<RichTextLabel>("Highscores");
				
				GameManager.SaveScore.LoadData();

				Array<int> highscores = GameManager.SaveScore.HighscoreData.Highscores;

				string highscoreText = $"\n\tTop 5 Scores:";

				for (int i = 0; i < 5; i++)
				{
					if (highscores.Count > i)
					{
						highscoreText += $"\n\t {i+1}. \t{highscores[highscores.Count - i - 1]}";
					}
					else
					{
						highscoreText += $"\n\t {i+1}. \t...";
					}
				}

				this._highscoresLabel.Text = highscoreText;
				break;
			case "2DGame":		// Setup the HUD and ScoreScreen for the main game
				this._gameplayHUD = GetNode("GameplayHUD") as GameplayHUD;
				this._scoreScreen = GetNode("ScoreScreen") as ScoreScreen;
				
				// Connect signals
				this._scoreScreen.ExitButton.Pressed += OnExitPressed;
				this._scoreScreen.MainMenuButton.Pressed += OnMainMenuPressed;
				this._scoreScreen.RetryButton.Pressed += OnStartGamePressed;
				
				TreeExited += DisconnectScoreScreenSignals;
				break;
		}
	}

	/// <summary>
	/// Starts the game by resetting variables and changing the scene
	/// </summary>
	public void OnStartGamePressed()
	{
		GameManager.instance.ResetVariables();
		GetTree().ChangeSceneToFile("res://Scenes/2d_game.tscn");
	}
	
	/// <summary>
	/// Exits the game
	/// </summary>
	public void OnExitPressed()
	{
		GetTree().Quit();
	}
	
	/// <summary>
	/// Goes to main menu by resetting variables and changing the scene
	/// </summary>
	public void OnMainMenuPressed()
	{
		GameManager.instance.ResetVariables();
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}
	
	/// <summary>
	/// Shows the score screen 
	/// </summary>
	public void ShowScoreScreen()
	{
		this._gameplayHUD.Visible = false;
		this._scoreScreen.Visible = true;
	
		int score = GameManager.instance.Score;
		this._scoreScreen.SetScoreLabelText(score, GameManager.SaveScore.IsHighscore(score));
	}
	
	/// <summary>
	/// Disconnects the signals for the scoreScreen buttons
	/// </summary>
	private void DisconnectScoreScreenSignals()
	{
		this._scoreScreen.ExitButton.Pressed -= OnExitPressed;
		this._scoreScreen.MainMenuButton.Pressed -= OnMainMenuPressed;
		this._scoreScreen.RetryButton.Pressed -= OnStartGamePressed;
		
		TreeExited -= DisconnectScoreScreenSignals;
	}
}
