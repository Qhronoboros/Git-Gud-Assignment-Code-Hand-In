using Godot;

// Service Locator
public partial class GameManager : Node
{
	public static GameManager instance { get; private set; }
	public static UIManager UIManager;
	public static SFXManager SFXManager;
	public static Conductor Conductor;
	public static PlayerController PlayerController;
	public static NoteSpawner NoteSpawner;
	public static SaveScore SaveScore;
	
	// ! Temporary
	public static LightHitAnimator lightHitAnimator;
	
	// Emits a signal whenever the score changes
	[Signal] public delegate void OnScoreChangeEventHandler(int value, bool increase);
	// Emits a signal whenever the combo value changes
	[Signal] public delegate void OnComboChangeEventHandler(int value, bool increase);
	// Emits a signal whenever the multiplier value changes
	[Signal] public delegate void OnMultiplierChangeEventHandler(int value, bool increase);
	
	private int _score = 0;
	public int Score
	{
		get { return this._score; }
		private set 
		{
			bool increase = value > this._score;
			this._score = value;
			EmitSignal(SignalName.OnScoreChange, this._score, increase);
		}
	}
	
	private int _combo = 0;
	public int Combo
	{
		get { return this._combo; }
		private set 
		{
			bool increase = value > this._combo;
			this._combo = value;
			EmitSignal(SignalName.OnComboChange, this._combo, increase);
		}
	}
	
	private int _multiplier = 1;
	public int Multiplier 
	{
		get { return this._multiplier; }
		private set 
		{
			bool increase = value > this._multiplier;
			this._multiplier = value;
			EmitSignal(SignalName.OnMultiplierChange, this._multiplier, increase);
		}
	}
	
	public float NotespeedMultiplier = 2.0f;
	public int JudgmentLinePosition = 100;

	public override void _Ready()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			GD.PrintErr($"A GameManager already exists, deleting self");
			QueueFree();
		}
	}
	
	/// <summary>
	/// Resets the game variables
	/// </summary>
	public void ResetVariables()
	{
		UIManager = null;
		SFXManager = null;
		Conductor = null;
		PlayerController = null;
		NoteSpawner = null;
		SaveScore = null;
		this.Score = this.Combo = 0;
		this.Multiplier = 1;
		
		this.NotespeedMultiplier = 2.0f;
		this.JudgmentLinePosition = 100;
	} 
	
	/// <summary>
	/// Adds a value to the score
	/// </summary>
	/// <param name="value">The value that is getting added to the score</param>
	public void AddScore(int value)
	{
		this.Score += value;
	}
	
	/// <summary>
	/// Sets the combo
	/// </summary>
	/// <param name="value">The new combo value</param>
	public void SetCombo(int value)
	{
		this.Combo = value;
	}
	
	/// <summary>
	/// When a note is getting hit, calculates the multiplier, adds score, and sets the combo
	/// </summary>
	/// <param name="points"></param>
	public void OnNoteHit(Vector2 position, int points)
	{
		int newMultiplier = this.Combo / 50 + 1;
		AddScore(points * newMultiplier);
		SetCombo(this.Combo + 1);
		
		if (newMultiplier != this.Multiplier)
			this.Multiplier = newMultiplier;
			
		// Special background effects at note location
		lightHitAnimator.AnimateLightAtLocation(position);
	}
	
	/// <summary>
	/// When the player misses a note, removes a value from the score and resets the combo and multiplier
	/// </summary>
	public void OnNoteMiss()
	{
		AddScore(-1);
		SetCombo(0);
		this.Multiplier = 1;
	}
}
