public class Program : Gear.Instance
{
	public override Program Create() => this;

	public override void EachTick(int tickCount)
	{
		if (tickCount == 1)
		{
			Gear.Sound.Play("bottle", loop: true);
			Gear.Sound.AddToCollection("asd", "bottle");
		}
	}
}