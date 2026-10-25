using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Services.Medications;
using CrudKronixForDevs.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Services
{
    public class MedicationRepositoryInMemoryTests
    {
        private readonly MedicationRepositoryInMemory _repository = new();

        private static Medication NewMedication(string name = "Dipirona")
        {
            var now = FixedTimeProvider.DefaultNow.UtcDateTime;
            return Medication.Add(name, "Princípio", 10m, 1, DateOnly.FromDateTime(now).AddYears(1), Guid.NewGuid(), now);
        }

        [Fact]
        public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            Assert.Empty(await _repository.GetAllAsync());
        }

        [Fact]
        public async Task AddAsync_ThenGetByIdAsync_ShouldReturnTheSameMedication()
        {
            var medication = NewMedication();

            await _repository.AddAsync(medication);
            var found = await _repository.GetByIdAsync(medication.Id);

            Assert.Same(medication, found); // em memória é exatamente o mesmo objeto
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEveryAddedMedication()
        {
            await _repository.AddAsync(NewMedication("A"));
            await _repository.AddAsync(NewMedication("B"));

            Assert.Equal(2, (await _repository.GetAllAsync()).Count);
        }

        [Fact]
        public async Task GetByIdAsync_UnknownId_ShouldReturnNull()
        {
            Assert.Null(await _repository.GetByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateAsync_ShouldKeepASingleRecord()
        {
            var medication = NewMedication();
            await _repository.AddAsync(medication);

            await _repository.UpdateAsync(medication);

            Assert.Single(await _repository.GetAllAsync());
        }

        [Fact]
        public async Task RemoveAsync_ExistingId_ShouldRemoveAndReturnTrue()
        {
            var medication = NewMedication();
            await _repository.AddAsync(medication);

            Assert.True(await _repository.RemoveAsync(medication.Id));
            Assert.Empty(await _repository.GetAllAsync());
        }

        [Fact]
        public async Task RemoveAsync_UnknownId_ShouldReturnFalse()
        {
            Assert.False(await _repository.RemoveAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnASnapshot()
        {
            // A lista devolvida é uma cópia: adicionar depois não altera quem já pegou a lista.
            await _repository.AddAsync(NewMedication("A"));
            var snapshot = await _repository.GetAllAsync();

            await _repository.AddAsync(NewMedication("B"));

            Assert.Single(snapshot);
        }
    }
}
