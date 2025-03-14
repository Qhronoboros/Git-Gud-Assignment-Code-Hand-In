using Godot;

public partial class GameplayHUD : CanvasLayer
{
	private Label _scoreLabel;
	private Label _comboLabel;
	private Label _multiplierLabel;
	
	public override void _Ready()
	{
		this._scoreLabel = GetNodeOrNull<Label>("ScoreLabel");
		this._comboLabel = GetNodeOrNull<Label>("ComboLabel");
		this._multiplierLabel = GetNodeOrNull<Label>("MultiplierLabel");
		
		// Connect signals for HUD value changes
		GameManager.instance.OnScoreChange += UpdateScore;
		GameManager.instance.OnComboChange += UpdateCombo;
		GameManager.instance.OnMultiplierChange += UpdateMultiplier;
		
		TreeExited += DisconnectHUDSignals;
		
		UpdateScore(GameManager.instance.Score);
	}

	/// <summary>
	/// Updates the scoreText shown in the game screen 
	/// </summary>
	/// <param name="value">The score value</param>
	public void UpdateScore(int value)
	{
		this._scoreLabel.Text = $"Score: {value}";
	}
	
	/// <summary>
	/// Updates the comboText shown in the game screen 
	/// </summary>
	/// <param name="value">The combo value</param>
	public void UpdateCombo(int value)
	{
		this._comboLabel.Text = $"Combo: {value}";
	}
	
	/// <summary>
	/// Updates the multiplierText shown in the game screen 
	/// </summary>
	/// <param name="value">The multiplier value</param>
	public void UpdateMultiplier(int value)
	{
		this._multiplierLabel.Text = $"Multiplier: {value}x";
	}
	
	/// <summary>
	/// Disconnects the HUD signals from the GameManager
	/// </summary>
	private void DisconnectHUDSignals()
	{
		GameManager.instance.OnScoreChange -= UpdateScore;
		GameManager.instance.OnComboChange -= UpdateCombo;
		GameManager.instance.OnMultiplierChange -= UpdateMultiplier;
		
		TreeExited -= DisconnectHUDSignals;
	}
}
