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
            var result = map.GetRelativePath(new FileId(1));
            result.IsFailure.Should().BeFalse();
            result.ValueOrThrow().Value.Should().Be("pathA");

            map.GetOrAddFileId(RelativePath.FromPath("pathB")).Id.Should().Be(2);
            result = map.GetRelativePath(new FileId(2));
            result.IsFailure.Should().BeFalse();
            result.ValueOrThrow().Value.Should().Be("pathB");

            map.GetOrAddFileId(RelativePath.FromPath("pathC")).Id.Should().Be(3);
            result = map.GetRelativePath(new FileId(3));
            result.IsFailure.Should().BeFalse();
            result.ValueOrThrow().Value.Should().Be("pathC");
        }

        [TestMethod]
        public void Same_Path_Always_Mapps_To_Same_Id()
        {
            var map = new FileMap();

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            var result = map.GetRelativePath(new FileId(1));
            result.IsFailure.Should().BeFalse();
            result.ValueOrThrow().Value.Should().Be("pathA");

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            result = map.GetRelativePath(new FileId(1));
            result.IsFailure.Should().BeFalse();
            result.ValueOrThrow().Value.Should().Be("pathA");

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            result = map.GetRelativePath(new FileId(1));
            result.IsFailure.Should().BeFalse();
            result.ValueOrThrow().Value.Should().Be("pathA");
        }

        [TestMethod]
        public void Rename_Fails_When_From_Does_Not_Exist()
        {
            var map = new FileMap();

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            var result = map.GetRelativePath(new FileId(1));
            result.IsFailure.Should().BeFalse();
            result.ValueOrThrow().Value.Should().Be("pathA");

            var renameResult = map.Rename(RelativePath.FromPath("not existing"), RelativePath.FromPath("new path"));
            renameResult.IsFailure.Should().BeTrue();

            result = map.GetRelativePath(new FileId(1));
            result.IsFailure.Should().BeFalse();
            result.ValueOrThrow().Value.Should().Be("pathA");

            result = map.GetRelativePath(new FileId(2));
            result.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void Rename_Existing_Path()
        {
            // Arrange
            var map = new FileMap();

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            var result = map.GetRelativePath(new FileId(1));
            result.IsFailure.Should().BeFalse();
            result.ValueOrThrow().Value.Should().Be("pathA");

            // Act
            var renameResult = map.Rename(RelativePath.FromPath("pathA"), RelativePath.FromPath("new path"));
            renameResult.IsFailure.Should().BeFalse();

            // Assert
            renameResult.ValueOrThrow().Id.Should().Be(1);

            result = map.GetRelativePath(new FileId(1));
            result.IsFailure.Should().BeFalse();
            result.ValueOrThrow().Value.Should().Be("new path");

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(2);
            result = map.GetRelativePath(new FileId(2));
            result.IsFailure.Should().BeFalse();
            result.ValueOrThrow().Value.Should().Be("pathA");
        }
    }
}
