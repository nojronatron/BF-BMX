using BFBMX.ServerApi.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BFBMX.Service.Test.Helpers
{
    public class BibRecordLoggerTests
    {
        private readonly NullLogger<BibRecordLogger> _moqLogster;
        private readonly Mock<IServerEnvFactory> _moqServerEnvFactory;
        private readonly BibRecordLogger _bibRecordLogger;

        public BibRecordLoggerTests()
        {
            _moqLogster = new NullLogger<BibRecordLogger>();
            _moqServerEnvFactory = new Mock<IServerEnvFactory>();
            _bibRecordLogger = new BibRecordLogger(_moqLogster, _moqServerEnvFactory.Object);
        }

        [Fact]
        public void InstantiatedBibRecordLoggerIsNotNull()
        {
            Assert.NotNull(_bibRecordLogger);
        }
    }
}
