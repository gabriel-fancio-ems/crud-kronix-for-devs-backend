using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Resources.OperationCenters;
using System.ComponentModel.DataAnnotations;

namespace CrudKronixForDevs.Core.Domains
{
    /// <summary>
    /// Entidade de domínio: representa um centro de operação, ao qual os medicamentos pertencem.
    ///
    /// Assim como em <see cref="Medication"/>, toda alteração passa por
    /// <see cref="Add(string, OperationCenterStatus, DateTime)"/> ou
    /// <see cref="Update(string, OperationCenterStatus)"/>, que validam os dados antes de aplicar.
    /// </summary>
    public class OperationCenter
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        [MaxLength(150)]
        public string Description { get; private set; } = string.Empty;

        public OperationCenterStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private OperationCenter() { }

        public static OperationCenter Add(string description, OperationCenterStatus status, DateTime createdAt)
        {
            var operationCenter = new OperationCenter
            {
                Id = Guid.NewGuid(),
                CreatedAt = createdAt
            };

            operationCenter.DefineData(description, status);

            return operationCenter;
        }

        public void Update(string description, OperationCenterStatus status)
        {
            DefineData(description, status);
        }

        private void DefineData(string description, OperationCenterStatus status)
        {
            // Regras de negócio: valem sempre, venha o dado de onde vier (API, teste, importação...).
            if (string.IsNullOrWhiteSpace(description)) throw new DomainException(OperationCenterMessages.DescriptionRequired);
            if (!Enum.IsDefined(status)) throw new DomainException(OperationCenterMessages.StatusInvalid);

            Description = description.Trim();
            Status = status;
        }

        public bool IsActive => Status == OperationCenterStatus.Active;
    }
}
