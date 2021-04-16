using FluentAssertions;
using LogMergeRx.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogMergeRx
{

    [TestClass]
    public class FileMapTests
    {
        [TestMethod]
        public void Different_Paths_Mapped_To_Different_Id()
        {
            var map = new FileMap();

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.TryGetRelativePath(new FileId(1), out var relativePath).Should().BeTrue();
            relativePath.Value.Should().Be("pathA");

            map.GetOrAddFileId(RelativePath.FromPath("pathB")).Id.Should().Be(2);
            map.TryGetRelativePath(new FileId(2), out relativePath).Should().BeTrue();
            relativePath.Value.Should().Be("pathB");

            map.GetOrAddFileId(RelativePath.FromPath("pathC")).Id.Should().Be(3);
            map.TryGetRelativePath(new FileId(3), out relativePath).Should().BeTrue();
            relativePath.Value.Should().Be("pathC");
        }

        [TestMethod]
        public void Same_Path_Always_Mapps_To_Same_Id()
        {
            var map = new FileMap();

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.TryGetRelativePath(new FileId(1), out var relativePath).Should().BeTrue();
            relativePath.Value.Should().Be("pathA");

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.TryGetRelativePath(new FileId(1), out relativePath).Should().BeTrue();
            relativePath.Value.Should().Be("pathA");

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.TryGetRelativePath(new FileId(1), out relativePath).Should().BeTrue();
            relativePath.Value.Should().Be("pathA");
        }

        [TestMethod]
        public void Rename_Fails_When_From_Does_Not_Exist()
        {
            var map = new FileMap();

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.TryGetRelativePath(new FileId(1), out var relativePath).Should().BeTrue();
            relativePath.Value.Should().Be("pathA");

            map.TryRename(RelativePath.FromPath("not existing"), RelativePath.FromPath("new path"), out var renamedFileId)
                .Should().BeFalse();

            map.TryGetRelativePath(new FileId(1), out relativePath).Should().BeTrue();
            relativePath.Value.Should().Be("pathA");
            map.TryGetRelativePath(new FileId(2), out relativePath).Should().BeFalse();
        }

        [TestMethod]
        public void Rename_Existing_Path()
        {
            // Arrange
            var map = new FileMap();

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.TryGetRelativePath(new FileId(1), out var relativePath).Should().BeTrue();
            relativePath.Value.Should().Be("pathA");

            // Act
            map.TryRename(RelativePath.FromPath("pathA"), RelativePath.FromPath("new path"), out var renamedFileId)
                .Should().BeTrue();

            // Assert
            renamedFileId.Id.Should().Be(1);

            map.TryGetRelativePath(new FileId(1), out relativePath).Should().BeTrue();
            relativePath.Value.Should().Be("new path");

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(2);
            map.TryGetRelativePath(new FileId(2), out relativePath).Should().BeTrue();
            relativePath.Value.Should().Be("pathA");
        }
    }
}
