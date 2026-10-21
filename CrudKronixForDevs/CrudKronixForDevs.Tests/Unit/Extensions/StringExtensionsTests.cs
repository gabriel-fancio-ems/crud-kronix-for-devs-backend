using CrudKronixForDevs.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("Paracetamol", "paracetamol")]
        [InlineData("ÁCIDO", "acido")]
        [InlineData("Ácido Acetilsalicílico", "acido acetilsalicilico")]
        [InlineData("  Dipirona  ", "dipirona")]
        [InlineData("Ação Çãõ", "acao cao")]
        public void NormalizeForSearch_ShouldRemoveAccentsSpacesAndCase(string input, string expected)
        {
            Assert.Equal(expected, input.NormalizeForSearch());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NormalizeForSearch_EmptyOrNull_ShouldReturnEmptyString(string? input)
        {
            Assert.Equal(string.Empty, input.NormalizeForSearch());
        }
    }
}
