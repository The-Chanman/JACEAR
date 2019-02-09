using System;

namespace Bose.Wearable
{
	/// <summary>
	/// Provides general-use utilities for working with the WearablePlugin.
	/// </summary>
	public static class WearableTools
	{
		/// <summary>
		/// Get the number of seconds between samples for a given <see cref="SensorUpdateInterval"/>.
		/// </summary>
		/// <param name="interval"></param>
		/// <returns></returns>
		public static float SensorUpdateIntervalToSeconds(SensorUpdateInterval interval)
		{
			// This is needed because the update interval enum doesn't reflect the actual sampling interval,
			// but the values of the underlying SDK enum.
			switch (interval)
			{
				case SensorUpdateInterval.TwentyMs:
					return 0.020f;
				case SensorUpdateInterval.FortyMs:
					return 0.040f;
				case SensorUpdateInterval.EightyMs:
					return 0.080f;
				case SensorUpdateInterval.OneHundredSixtyMs:
					return 0.160f;
				case SensorUpdateInterval.ThreeHundredTwentyMs:
					return 0.320f;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
				/// <summary>
		/// Get the number of milliseconds between samples for a given <see cref="SensorUpdateInterval"/>.
		/// </summary>
		/// <param name="interval"></param>
		/// <returns></returns>
		public static float SensorUpdateIntervalToMilliseconds(SensorUpdateInterval interval)
		{
			return SensorUpdateIntervalToSeconds(interval) * 1000.0f;
		}

		internal static ProductType GetProductType(ProductId productId)
		{
			var productType = ProductType.Unknown;
			switch (productId)
			{
				case ProductId.BoseFrames:
					productType = ProductType.BoseFrames;
					break;
			}

			return productType;
		}

		public static VariantType GetVariantType(ProductType productType, byte variantId)
		{
			var variantType = VariantType.Unknown;
			switch (productType)
			{
				case ProductType.BoseFrames:
					variantType = GetBoseFramesVariantType(variantId);
					break;
			}

			return variantType;
		}

		private static VariantType GetBoseFramesVariantType(byte variantId)
		{
			var variantType = VariantType.Unknown;
			var boseFramesVariant = (BoseFramesVariantID)variantId;
			if (Enum.IsDefined(typeof(BoseFramesVariantID), boseFramesVariant))
			{
				switch (boseFramesVariant)
				{
					case BoseFramesVariantID.BoseFramesAlto:
						variantType = VariantType.BoseFramesAlto;
						break;
					case BoseFramesVariantID.BoseFramesRondo:
						variantType = VariantType.BoseFramesRondo;
						break;
				}
			}

			return variantType;
		}
	}
}
