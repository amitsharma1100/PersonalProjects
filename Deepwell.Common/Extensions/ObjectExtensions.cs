namespace Deepwell.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object value)
        {
            return value == null;
        }

        public static bool IsNotNull(this object value)
        {
            return (value == null) == false;
        }
    }
}
