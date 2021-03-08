public class Program : Gear.Instance
{
	public override Program Create() => this;

	public override void EachTick(int tickCount)
	{
		if (tickCount == 1)
		{
			Gear.Sound.AddToCollection("asd", "bottle");
		}
	}
}