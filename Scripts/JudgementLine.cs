using Godot;

public partial class JudgementLine : Line2D
{
	/// <summary>
	/// Sets the judgmentLines position
	/// </summary>
	public override void _Ready()
	{
		GlobalPosition = new Vector2(GlobalPosition.X, GetViewport().GetVisibleRect().Size.Y - GameManager.instance.JudgmentLinePosition);
	}
}
