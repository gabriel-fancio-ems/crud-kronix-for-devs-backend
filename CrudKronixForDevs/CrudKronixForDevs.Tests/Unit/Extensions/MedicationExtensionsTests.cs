using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Extensions;
using CrudKronixForDevs.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Extensions
{
    public class MedicationExtensionsTests
    {
        [Fact]
        public void ToResponse_ShouldCopyEveryField()
        {
            var now = FixedTimeProvider.DefaultNow.UtcDateTime;
            var expiration = DateOnly.FromDateTime(now).AddYears(1);
            var medication = Medication.Add("Dipirona", "Dipirona Monoidratada", 12.90m, 150, expiration, now);

            var response = medication.ToResponse();

            Assert.Equal(medication.Id, response.Id);
            Assert.Equal("Dipirona", response.Name);
            Assert.Equal("Dipirona Monoidratada", response.ActiveIngredient);
            Assert.Equal(12.90m, response.Price);
            Assert.Equal(150, response.StockQuantity);
            Assert.Equal(expiration, response.ExpirationDate);
            Assert.Equal(now, response.CreatedAt);
        }
    }

}
