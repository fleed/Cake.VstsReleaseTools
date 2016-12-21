namespace Cake.VstsReleaseNotes.Tests
{
    using Core.Diagnostics;

    using Moq;

    using Newtonsoft.Json;

    using VstsReleaseTools.Entities;

    using Xunit;

    public class SerializationTests
    {
        [Fact]
        public void TestFieldsSerialization()
        {
            var json = @"{""System.Title"":null,""Vsts.ReleaseNotes"":""something"", ""System.WorkItemType"":""Bug""}";
            var log = new Mock<ICakeLog>();
            var fields = JsonConvert.DeserializeObject<Fields>(
                json,
                new JsonSerializerSettings { ContractResolver = new ReleaseNotesContractResolver(log.Object, "Vsts.ReleaseNotes") });
            Assert.Null(fields.Title);
            Assert.Equal("Bug", fields.Type);
            Assert.Equal("something", fields.ReleaseNotes);
        }
    }
}