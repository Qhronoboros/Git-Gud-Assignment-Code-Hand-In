// Main Source: Complete Godot Rhythm Game Tutorial - LegionGames 
// https://www.youtube.com/watch?v=_FRiPPbJsFQ

using Godot;
using System;

public partial class Conductor : AudioStreamPlayer
{
	[Export] private float _bpm = 87.5f;
	
	// The given starting point of the chart in seconds
	[Export] private float _startPoint = 0.0f;
	
	// Uses Godot's built-in Timer Node
	private Timer _startTimer;
	
	// The amplified speed of the falling notes (amplified by the GameManager)
	public float AmplifiedNoteSpeed;
	public double SecondsPerBeat;
	private int _currentSongBeat = -1;
	
	// How long it takes for a note to reach the judgmentLine
	private double _noteSpawnTimeOffset;
	
	// Is used for spawning notes before the song officially starts
	private bool _simulateStart = true;
	
	// Emits a signal whenever there is a new beat
	[Signal] public delegate void OnNewBeatEventHandler(float currentBeat);
	
	public override void _Ready()
	{
		if (GameManager.Conductor == null)
		{
			GameManager.Conductor = this;
		}
		else 
		{
			GD.PrintErr($"A Conductor already exists, deleting self");
			QueueFree();
		}
		
		this.SecondsPerBeat = 60.0 / this._bpm;
		this.AmplifiedNoteSpeed = Note.FallSpeed * GameManager.instance.NotespeedMultiplier;
		
		// Get how many seconds it takes for a note to reach the judgementLine (travel distance / note fallspeed per second)
		this._noteSpawnTimeOffset = (GetViewport().GetVisibleRect().Size.Y - GameManager.instance.JudgmentLinePosition + 32.0f) / this.AmplifiedNoteSpeed;
		
		if (this._startPoint != 0.0f)
		{
			PlayAtPoint(this._startPoint);
		}
		else 
		{
			this._startTimer = GetNode<Timer>("StartTimer");
			this._startTimer.WaitTime = this._noteSpawnTimeOffset;
			this._startTimer.Start();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		ProcessSongPosition();
	}
	
	/// <summary>
	/// Gets the current songPosition and songBeat, it then emits a signal OnNewBeat when the songBeat value is new
	/// </summary>
	private void ProcessSongPosition()
	{
		if (!Playing && !this._simulateStart)
			return;
		
		// Get the playback position, add the time since the last mixed audio chunk, subtract the output latency, and add the noteSpawnTimeOffset
		double songPosition = AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency() + this._noteSpawnTimeOffset;
		
		if (Playing)
		{
			songPosition += GetPlaybackPosition();
			if (this._simulateStart)
				this._simulateStart = false;
		}
		else
		{
			songPosition -= this._startTimer.TimeLeft;
		}
		
		int songBeat = (int)(songPosition / this.SecondsPerBeat);
		
		// Temporary button for checking the current beat when creating chart/beatmap
		if (Input.IsActionJustPressed("Get Current Beat"))
			GD.Print(MathF.Round((float)(songPosition / this.SecondsPerBeat - 1.25f), 2));
		
		if (songBeat != this._currentSongBeat)
		{
			// GD.Print($"Beat: {songBeat}");
			this._currentSongBeat = songBeat;
			EmitSignal(SignalName.OnNewBeat, this._currentSongBeat);
		}
	}
	
	/// <summary>
	/// Plays the song at the given startPoint
	/// </summary>
	/// <param name="startPoint">The given starting point of the chart in seconds</param>
	private void PlayAtPoint(float startPoint)
	{
		Play();
		Seek(startPoint);
	}
	
	/// <summary>
	/// Song starts when the timer ends (Is currently not getting used)
	/// </summary>
	private void OnStartTimerTimeout()
	{
		this._startTimer.Stop();
		Play();
	}
}
