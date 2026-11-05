using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Resources.Equipments;
using System.ComponentModel.DataAnnotations;

namespace CrudKronixForDevs.Core.Domains
{
    /// <summary>
    /// Entidade de domínio: representa um equipamento.
    ///
    /// A criação passa por <see cref="Add(string, string, EquipmentStatus, DateTime)"/>,
    /// que valida os dados antes de aplicar.
    /// </summary>
    public class Equipment
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; private set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Description { get; private set; } = string.Empty;

        public EquipmentStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private Equipment() { }

        public static Equipment Add(string code, string description, EquipmentStatus status, DateTime createdAt)
        {

            var equipment  = new Equipment
            {
                Id = Guid.NewGuid(),
                Code = code.Trim(),
                Description = description.Trim(),
                Status = status,
                CreatedAt = createdAt
            };

             equipment.DefineData(code, description, status);

            return equipment;
        }

        
        public void Update(string code, string description, EquipmentStatus status)
        {
            DefineData(code, description, status);
        }

        
        private void DefineData(string code, string description, EquipmentStatus status)
        {
            // Regras de negócio: valem sempre, venha o dado de onde vier (API, teste, importação...).
            if (string.IsNullOrWhiteSpace(code)) throw new DomainException(EquipmentMessages.CodeRequired);
            if (string.IsNullOrWhiteSpace(description)) throw new DomainException(EquipmentMessages.DescriptionRequired);
            if (!Enum.IsDefined(status)) throw new DomainException(EquipmentMessages.StatusInvalid);

            Code = code.Trim();
            Description = description.Trim();
            Status = status;
        }


        public bool IsActive => Status == EquipmentStatus.Active;
    }
}
