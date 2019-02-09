using System;

namespace Bose.Wearable
{
	/// <summary>
	/// The variant ID of a BoseFrames hardware device.
	/// </summary>
	[Serializable]
	public enum BoseFramesVariantID : byte
	{
		Undefined = 0,
		BoseFramesAlto = 0x01,
		BoseFramesRondo = 0x02
	}
}
