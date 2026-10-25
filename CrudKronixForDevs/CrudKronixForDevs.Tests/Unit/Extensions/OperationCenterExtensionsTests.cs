using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Extensions;
using CrudKronixForDevs.Tests.Fakes;

namespace CrudKronixForDevs.Tests.Unit.Extensions
{
    public class OperationCenterExtensionsTests
    {
        [Fact]
        public void ToResponse_ShouldCopyEveryField()
        {
            var now = FixedTimeProvider.DefaultNow.UtcDateTime;
            var operationCenter = OperationCenter.Add("Centro Principal", OperationCenterStatus.Active, now);

            var response = operationCenter.ToResponse();

            Assert.Equal(operationCenter.Id, response.Id);
            Assert.Equal("Centro Principal", response.Description);
            Assert.Equal(OperationCenterStatus.Active, response.Status);
            Assert.Equal(now, response.CreatedAt);
        }
    }
}
