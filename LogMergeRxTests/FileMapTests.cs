using FluentAssertions;
using LogMergeRx.Model;
using Xunit;

namespace LogMergeRx
{
    public class FileMapTests
    {
        [Fact]
        public void Different_Paths_Mapped_To_Different_Id()
        {
            var map = new FileMap();

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.GetRelativePath(new FileId(1)).ValueOrThrow().ToString().Should().Be("pathA");

            map.GetOrAddFileId(RelativePath.FromPath("pathB")).Id.Should().Be(2);
            map.GetRelativePath(new FileId(2)).ValueOrThrow().ToString().Should().Be("pathB");

            map.GetOrAddFileId(RelativePath.FromPath("pathC")).Id.Should().Be(3);
            map.GetRelativePath(new FileId(3)).ValueOrThrow().ToString().Should().Be("pathC");
        }

        [Fact]
        public void Same_Path_Always_Mapps_To_Same_Id()
        {
            var map = new FileMap();

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.GetRelativePath(new FileId(1)).ValueOrThrow().ToString().Should().Be("pathA");

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.GetRelativePath(new FileId(1)).ValueOrThrow().ToString().Should().Be("pathA");

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.GetRelativePath(new FileId(1)).ValueOrThrow().ToString().Should().Be("pathA");
        }

        [Fact]
        public void Rename_Fails_When_From_Does_Not_Exist()
        {
            var map = new FileMap();

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.GetRelativePath(new FileId(1)).ValueOrThrow().ToString().Should().Be("pathA");

            map .Rename(RelativePath.FromPath("not existing"), RelativePath.FromPath("new path"))
                .ErrorsOrEmpty().Should().NotBeEmpty();

            map.GetRelativePath(new FileId(1)).ValueOrThrow().ToString().Should().Be("pathA");

            map.GetRelativePath(new FileId(2)).ErrorsOrEmpty().Should().NotBeEmpty();
        }

        [Fact]
        public void Rename_Existing_Path()
        {
            // Arrange
            var map = new FileMap();

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(1);
            map.GetRelativePath(new FileId(1)).ValueOrThrow().ToString().Should().Be("pathA");

            // Act
            map.Rename(RelativePath.FromPath("pathA"), RelativePath.FromPath("new path"))
                .ValueOrThrow().Should().Be(new FileId(1));

            map.GetRelativePath(new FileId(1)).ValueOrThrow().ToString().Should().Be("new path");

            map.GetOrAddFileId(RelativePath.FromPath("pathA")).Id.Should().Be(2);
            map.GetRelativePath(new FileId(2)).ValueOrThrow().ToString().Should().Be("pathA");
        }
    }
}
