using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CrudKronixForDevs.Core.Extensions
{
    /// <summary>
    /// Deixa o texto em minúsculas e sem acentos, para comparações e buscas.
    /// Exemplo: "Paracetamol Ácido" vira "paracetamol acido".
    /// </summary>
    public static class StringExtensions
    {
        public static string NormalizeForSearch(this string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var decomposed = text.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                    builder.Append(character);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }
    }
}
