using System.Globalization;

namespace WheelOfFortune.UI.Common
{
    public static class AmountFormatter
    {
        private const int Million = 1_000_000;
        private const int ShortenThreshold = 10_000;
        private const int Thousand = 1_000;

        public static string Format(int amount)
        {
            if (amount >= Million)
            {
                return (amount / (float)Million).ToString("0.#", CultureInfo.InvariantCulture) + "M";
            }

            if (amount >= ShortenThreshold)
            {
                return (amount / (float)Thousand).ToString("0.#", CultureInfo.InvariantCulture) + "K";
            }

            return amount.ToString(CultureInfo.InvariantCulture);
        }

        public static string FormatMultiplier(int amount) => "x" + Format(amount);
    }
}
