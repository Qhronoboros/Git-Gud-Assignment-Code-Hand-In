using Godot;

public partial class ScoreScreen : Control
{
	public RichTextLabel ScoreLabel;
	
	public Button ExitButton;
	public Button MainMenuButton;
	public Button RetryButton;
	
	public override void _Ready()
	{
		ScoreLabel = GetNodeOrNull<RichTextLabel>("PanelContainer/VBoxContainer/ScoreText");
		
		ExitButton = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/MarginContainer/HBoxContainer/Exit");
		MainMenuButton = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/MarginContainer/HBoxContainer/MainMenu");
		RetryButton = GetNodeOrNull<Button>("PanelContainer/VBoxContainer/MarginContainer/HBoxContainer/Retry");
	}
	
	/// <summary>
	/// Sets the score text at the end of the level (score screen)
	/// </summary>
	/// <param name="score">The player's score</param>
	/// <param name="IsHighscore">Whether the score is a highscore</param>
	public void SetScoreLabelText(int score, bool IsHighscore)
	{
		string scoreText = $"\tScore: {score}";
		if (IsHighscore)
			scoreText += $"\t\t\tHighscore!";
		
		ScoreLabel.Text = scoreText;
	}
}
