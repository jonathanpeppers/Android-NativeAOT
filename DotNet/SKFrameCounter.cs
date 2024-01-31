namespace SkiaSharp;

/// <summary>
/// From: https://github.com/mono/SkiaSharp.Extended/blob/93efd2d38783d6d8f638623327e7f9749cea70e2/source/SkiaSharp.Extended.UI/Utils/SKFrameCounter.shared.cs
/// </summary>
class SKFrameCounter
{
	private bool firstRender = true;
	private int lastTick;

	public SKFrameCounter()
	{
		Reset();
	}

	public TimeSpan Duration { get; private set; }

    public TimeSpan TotalDuration { get; private set; }

	public void Reset()
	{
		firstRender = true;

		Duration = TimeSpan.Zero;
	}

	public TimeSpan NextFrame()
	{
		if (firstRender)
		{
			lastTick = Environment.TickCount;
			firstRender = false;
		}

		var ticks = Environment.TickCount;
		var delta = ticks - lastTick;
		lastTick = ticks;

		Duration = TimeSpan.FromMilliseconds(delta);
        TotalDuration += Duration;
		return Duration;
	}
}