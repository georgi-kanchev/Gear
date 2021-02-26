public class Program : Gear.Instance
{
	public override Program Create() => this;

	public override void EachTick(int tickCount)
	{
		if (tickCount == 1)
		{
			Gear.Canvas.SetPixelSize(5, 5);

			var asd = new Gear.Storage<string, string>();
			asd.Expand(10, "key", "value");
			asd.Expand(10, "key2", "value2");
			asd.ReplaceAt(10, "test");

			var indexes = asd.GetIndexes();
			var keys = asd.GetUniqueKeys();
			var values = asd.GetValues();
			asd.Free();
			for (int i = 0; i < asd.GetDataAmount(); i++)
			{
				var index = indexes[i];
				var key = asd.GetUniqueKeyAt(index);
				var value = asd.GetValueAt(index);
				Gear.Text.Display("font", $"{index} {key} {value}\n", scale: 0.5f);
			}
		}
		if (Gear.Input.KeyWasJustPressed(Gear.Keys.UpArrow))
		{
			Gear.Text.Display("font", tickCount + "\n");
		}
	}
}