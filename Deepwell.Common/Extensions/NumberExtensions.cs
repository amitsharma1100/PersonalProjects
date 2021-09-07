namespace Deepwell.Common.Extensions
{
    public static class NumberExtensions
    {
        public static string FormatCurrency(this decimal amount)
        {
            return string.Format("{0:C}", amount);
        }
    }
}
