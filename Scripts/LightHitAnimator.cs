using Godot;
using System;

public partial class LightHitAnimator : PointLight2D
{
	private AnimationPlayer _lightAnimator;
	
	public override void _Ready()
	{
        if (GameManager.lightHitAnimator == null)
		{
			GameManager.lightHitAnimator = this;
		}
		else
		{
			GD.PrintErr($"A LightHitAnimator already exists, deleting self");
			QueueFree();
		}
		
        Visible = false;
		
		this._lightAnimator = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
	}
	
	public void AnimateLightAtLocation(Vector2 position)
	{
        this.GlobalPosition = position;
	
        if (this._lightAnimator.IsPlaying())
            this._lightAnimator.Stop();

	    this._lightAnimator.Play("LightAnim");
	}
}
