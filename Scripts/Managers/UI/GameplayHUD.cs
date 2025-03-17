using Godot;

public partial class GameplayHUD : CanvasLayer
{
	private Label _scoreLabel;
	private Label _comboLabel;
	private Label _multiplierLabel;
	
	private AnimationPlayer _scoreAnimator;
	private AnimationPlayer _comboAnimator;
	private AnimationPlayer _multiplierAnimator;
	
	public override void _Ready()
	{
		this._scoreLabel = GetNodeOrNull<Label>("ScoreLabel");
		this._comboLabel = GetNodeOrNull<Label>("ComboLabel");
		this._multiplierLabel = GetNodeOrNull<Label>("MultiplierLabel");
		
		this._scoreAnimator = this._scoreLabel.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		this._comboAnimator = this._comboLabel.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		this._multiplierAnimator = this._multiplierLabel.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		
		// Connect signals for HUD value changes
		GameManager.instance.OnScoreChange += UpdateScore;
		GameManager.instance.OnComboChange += UpdateCombo;
		GameManager.instance.OnMultiplierChange += UpdateMultiplier;
		
		TreeExited += DisconnectHUDSignals;
		
		// UpdateScore(GameManager.instance.Score);
	}

	/// <summary>
	/// Updates the scoreText shown in the game screen 
	/// </summary>
	/// <param name="value">The score value</param>
	public void UpdateScore(int value, bool increase)
	{
		this._scoreLabel.Text = $"Score: {value}";
		
		if (increase) 
		{
		    // Do special effect when it's an increase
		    // Also a different effect depending on the combo amount
		    PlayUIAnimation(this._scoreAnimator, "UIHitAnim");
		}
	}
	
	/// <summary>
	/// Updates the comboText shown in the game screen 
	/// </summary>
	/// <param name="value">The combo value</param>
	public void UpdateCombo(int value, bool increase)
	{
		this._comboLabel.Text = $"Combo: {value}";
		
		if (increase) 
		{
		    // Do special effect when it's an increase
		    // Also a different effect depending on the combo amount
		    PlayUIAnimation(this._comboAnimator, "UIHitAnim");
		}
	}
	
	/// <summary>
	/// Updates the multiplierText shown in the game screen 
	/// </summary>
	/// <param name="value">The multiplier value</param>
	public void UpdateMultiplier(int value, bool increase)
	{
		this._multiplierLabel.Text = $"Multiplier: {value}x";
		
		if (increase) 
		{
		    // Do special effect when it's an increase
		    // Also a different effect depending on the combo amount
		    PlayUIAnimation(this._multiplierAnimator, "UIHitAnim");
		}
	}
	
	/// <summary>
	/// Plays the UI animation with the given animationName
	/// </summary>
	/// <param name="animationPlayer">The animationPlayer of the UI element</param>
	/// <param name="animationName">The name of the animation</param>
	private void PlayUIAnimation(AnimationPlayer animationPlayer, string animationName)
	{
		if (animationPlayer.IsPlaying())
			animationPlayer.Stop();		    

		animationPlayer.Play(animationName);
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
