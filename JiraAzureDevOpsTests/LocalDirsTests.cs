using Jira2AzureDevOps.Logic;
using Jira2AzureDevOps.Logic.Jira.Model;
using NUnit.Framework;
using Shouldly;
using System.IO;

namespace JiraAzureDevOpsTests
{
    [TestFixture]
    public class LocalDirsTests
    {
        private readonly char _s = Path.DirectorySeparatorChar;
        private readonly Attachment _attachment = new Attachment { Id = 333, Filename = "lala.png" };

        [Test]
        public void GetAttachmentIdFromPath_WorksForRelativePath()
        {
            var pwd = Directory.GetCurrentDirectory();
            var localDirs = new LocalDirs(pwd);
            var path = localDirs.GetAttachmentFile(_attachment).FullName;
            localDirs.GetAttachmentIdFromPath(path).ShouldBe(_attachment.Id.ToString());
        }

        [Test]
        public void GetAttachmentIdFromPath_WorksForAbsolutePath()
        {
            var pwd = Directory.GetCurrentDirectory();
            var localDirs = new LocalDirs(pwd);
            var file = localDirs.GetAttachmentFile(_attachment);
            var path = localDirs.GetRelativePath(file);
            localDirs.GetAttachmentIdFromPath(path).ShouldBe(_attachment.Id.ToString());
        }

        [Test]
        public void GetRelativePath_RemovesWorkingDirectory()
        {
            var pwd = Directory.GetCurrentDirectory();
            var localDirs = new LocalDirs(pwd);
            var file = localDirs.GetAttachmentFile(_attachment);

            var path = localDirs.GetRelativePath(file);
            path.ShouldContain($"{_s}{_attachment.Id.ToString()}{_s}");
            path.ShouldContain($"{_s}{_attachment.Filename}");

            var newFile = localDirs.GetFileFromRelativePath(path);
            newFile.FullName.ShouldBe(file.FullName);
        }
    }
}
