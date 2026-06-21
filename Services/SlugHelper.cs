using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NguyenDinhMinhKhang_2380600989.Services
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            // Chuyển tiếng Việt có dấu thành không dấu
            string normalized = RemoveDiacritics(text);

            // Chuyển thành chữ thường
            normalized = normalized.ToLowerInvariant();

            // Thay khoảng trắng và ký tự đặc biệt bằng dấu gạch ngang
            normalized = Regex.Replace(normalized, @"[^a-z0-9\s-]", "");
            normalized = Regex.Replace(normalized, @"\s+", "-").Trim('-');

            return normalized;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}