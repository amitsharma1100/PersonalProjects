namespace Deepwell.Common.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNull(this string value)
        {
            return value == null
                ? true
                : false;
        }

        public static bool IsNullOrEmpty(this string value)
        {
            if (value.IsNull())
            {
                return true;
            }

            if (value.Equals(string.Empty))
            {
                return true;
            }

            return false;
        }

        public static bool HasValue(this string value)
        {
            return (value.IsNullOrEmpty() == false && value.Length > 0);               
        }

        public static bool HasNoValue(this string value)
        {
            return HasValue(value) == false;
        }

        public static bool IsNotEqualTo(this string value, string valueToCompare)
        {
            return value.IsNotNull() && valueToCompare.IsNotNull() && (value.Equals(valueToCompare) == false);
        }
    }
}
