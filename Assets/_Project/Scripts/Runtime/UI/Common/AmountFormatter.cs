using System.Globalization;

namespace WheelOfFortune.UI.Common
{
    public static class AmountFormatter
    {
        #region Fields

        private const int MILLION = 1_000_000;
        private const int SHORTEN_THRESHOLD = 10_000;
        private const int THOUSAND = 1_000;
        private const string MULTIPLIER_PREFIX = "x";

        #endregion

        #region Public Methods

        public static string Format(int amount)
        {
            if (amount >= MILLION)
            {
                return (amount / (float)MILLION).ToString("0.#", CultureInfo.InvariantCulture) + "M";
            }

            if (amount >= SHORTEN_THRESHOLD)
            {
                return (amount / (float)THOUSAND).ToString("0.#", CultureInfo.InvariantCulture) + "K";
            }

            return amount.ToString(CultureInfo.InvariantCulture);
        }

        public static string FormatMultiplier(int amount)
        {
            return MULTIPLIER_PREFIX + Format(amount);
        }

        #endregion
    }
}
