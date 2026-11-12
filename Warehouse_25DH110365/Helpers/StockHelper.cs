using System.Globalization;

namespace Warehouse_25DH110365.Helpers
{
    public static class StockHelper
    {
        public static string GetStatus(int q)
        {
            if (q <= 0) return "Hết hàng";
            if (q <= 4) return "Sắp hết";
            return "Đủ hàng";
        }

        public static bool TryParsePrice(string s, out decimal v)
        {
            v = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            return decimal.TryParse(s.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out v);
        }

        // số nguyên không âm
        public static bool TryParseQty(string s, out int v)
        {
            v = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            return int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out v) && v >= 0;
        }
    }
}