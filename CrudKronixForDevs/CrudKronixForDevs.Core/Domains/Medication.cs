using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Resources.Medications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CrudKronixForDevs.Core.Domains
{
    /// <summary>
    /// Entidade de domínio: representa um medicamento e protege as próprias regras.
    ///
    /// Repare que os "set" são privados. Ninguém de fora consegue deixar um medicamento
    /// em estado inválido, porque toda alteração passa por <see cref="Add(string, string, string, decimal, int, DateOnly, DateTime)"/> ou
    /// <see cref="Update(string, string, string, decimal, int, DateOnly, DateTime)"/>, que validam os dados antes de aplicar.
    /// </summary>
    /// 
    public class Medication
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ActiveIngredient { get; set; } = string.Empty;

        [Range(0.01, 999999999.99)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public DateOnly ExpirationDate { get; set; }
        public DateTime CreatedAt { get; private set; }

        private Medication() { }

        public static Medication Add(string name, string activeIngredient, decimal price, int stockQuantity, DateOnly expirationDate, DateTime createdAt)
        {
            var medication = new Medication
            {
                Id = Guid.NewGuid(),
                CreatedAt = createdAt
            };

            medication.DefineData(name, activeIngredient, price, stockQuantity,
                expirationDate, DateOnly.FromDateTime(createdAt));

            return medication;
        }

        public void Update(string name, string activeIngredient, decimal price, int stockQuantity, DateOnly expirationDate, DateTime createdAt)
        {
            DefineData(name, activeIngredient, price, stockQuantity,
                expirationDate, DateOnly.FromDateTime(createdAt));
        }

        private void DefineData(string name, string activeIngredient, decimal price, int stockQuantity, DateOnly expirationDate, DateOnly now)
        {
            // Regras de negócio: valem sempre, venha o dado de onde vier (API, teste, importação...).
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException(MedicationMessages.NameRequired);
            if (string.IsNullOrWhiteSpace(activeIngredient)) throw new DomainException(MedicationMessages.ActiveIngredientRequired);
            if (price <= 0) throw new DomainException(MedicationMessages.PriceInvalid);
            if (stockQuantity < 0) throw new DomainException(MedicationMessages.StockInvalid);
            if (expirationDate <= now) throw new DomainException(MedicationMessages.ExpirationDateMustBeFuture);

            Name = name.Trim();
            ActiveIngredient = activeIngredient.Trim();
            Price = price;
            StockQuantity = stockQuantity;
            ExpirationDate = expirationDate;
        }

        public bool IsExpired(DateOnly now) => ExpirationDate < now;

        public bool ExpiresSoon(int days, DateOnly now) =>
            !IsExpired(now) && ExpirationDate <= now.AddDays(days);
    }
}