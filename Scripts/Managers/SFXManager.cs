using Godot;
using Godot.Collections;

public partial class SFXManager : Node
{
	// Dictionary of sounds with their corresponding name as keys
	private Dictionary<string, AudioStreamPlayer> _sfxDictionary = new Dictionary<string, AudioStreamPlayer>(); 
	
	public override void _Ready()
	{
		if (GameManager.SFXManager == null)
		{
			GameManager.SFXManager = this;
		}
		else 
		{
			GD.PrintErr($"A SFXManager already exists, deleting self");
			QueueFree();
		}
		
		foreach(AudioStreamPlayer sfxPlayer in GetChildren())
		{
			this._sfxDictionary.Add(sfxPlayer.Name, sfxPlayer);
		}
		
		GD.Print($"SFXDictionary Count: {this._sfxDictionary.Count}");
	}
	
	/// <summary>
	/// Plays a sound with the given soundName
	/// </summary>
	/// <param name="soundName">The name of the sound</param>
	public void PlaySound(string soundName)
	{
		this._sfxDictionary.TryGetValue(soundName, out AudioStreamPlayer sfxPlayer);
		if (sfxPlayer != null)
		{
			sfxPlayer.Play();
		}
		else
		{
			GD.PrintErr($"Could not find given sound effect: {soundName}");
		}
	}
}
