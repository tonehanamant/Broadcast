﻿using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class PostServiceTests
    {
        [Test]
        public void SavePostFile_Throws_When_File_Not_Excel()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostService>();

            var request = new PostRequest();
            request.FileName = "abc.notexcel";

            Assert.That(() => sut.SavePost(request), Throws.Exception.With.Message.EqualTo(PostService.FileNotExcelErroMessage));
        }

        [Test]
        public void SavePostFile_Throws_When_Audiences_Null()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostService>();

            var request = new PostRequest();
            request.FileName = "abc.xlsx";
            request.Audiences = null;

            Assert.That(() => sut.SavePost(request), Throws.Exception.With.Message.EqualTo(PostService.NoAudiencesErrorMessage));
        }

        [Test]
        public void SavePostFile_Throws_When_Invalid_PlaybackType()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostService>();

            var request = new PostRequest();
            request.FileName = "abc.xlsx";
            request.Audiences = new List<int> { 1 };
            request.PlaybackType = (ProposalEnums.ProposalPlaybackType)'a';

            Assert.That(() => sut.SavePost(request), Throws.Exception.With.Message.EqualTo(string.Format(PostService.InvalidPlaybackTypeErrorMessage, (char)request.PlaybackType)));
        }

        [Test]
        public void SavePostFile_Throws_When_FileName_Already_Exist()
        {
            var mocker = new AutoMoq.AutoMoqer();
            var postRepository = mocker.GetMock<IPostRepository>();
            postRepository.Setup(r => r.PostExist(It.IsAny<string>())).Returns(true);
            var broadcastDataRepository = mocker.GetMock<IDataRepositoryFactory>();
            broadcastDataRepository.Setup(r => r.GetDataRepository<IPostRepository>()).Returns(postRepository.Object);

            var sut = mocker.Create<PostService>();

            var request = new PostRequest();
            request.FileName = "abc.xlsx";
            request.Audiences = new List<int> { 1 };
            request.PlaybackType = ProposalEnums.ProposalPlaybackType.Live;

            Assert.That(() => sut.SavePost(request), Throws.Exception.With.Message.EqualTo(string.Format(PostService.DuplicateFileErrorMessage, request.FileName)));
        }

        [Test]
        public void EditPostFile_Throws_When_Audiences_Null()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostService>();

            var request = new PostRequest();
            request.FileId = 1;
            request.FileName = "abc.xlsx";
            request.Audiences = null;

            Assert.That(() => sut.EditPost(request), Throws.Exception.With.Message.EqualTo(PostService.NoAudiencesErrorMessage));
        }

        [Test]
        public void EditPostFile_Throws_When_Invalid_PlaybackType()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostService>();

            var request = new PostRequest();
            request.FileId = 1;
            request.FileName = "abc.xlsx";
            request.Audiences = new List<int> { 1 };
            request.PlaybackType = (ProposalEnums.ProposalPlaybackType)'a';

            Assert.That(() => sut.EditPost(request), Throws.Exception.With.Message.EqualTo(string.Format(PostService.InvalidPlaybackTypeErrorMessage, (char)request.PlaybackType)));
        }

        [Test]
        public void EditPostFile_Throws_When_No_Id()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostService>();

            var request = new PostRequest();
            request.FileId = null;

            Assert.That(() => sut.EditPost(request), Throws.Exception.With.Message.EqualTo(PostService.MissingId));
        }

        [Test]
        public void Edit_Throws_When_FileName_Already_Exist()
        {
            var mocker = new AutoMoq.AutoMoqer();
            var postRepository = mocker.GetMock<IPostRepository>();
            postRepository.Setup(r => r.PostExist(It.IsAny<string>())).Returns(true);
            var broadcastDataRepository = mocker.GetMock<IDataRepositoryFactory>();
            broadcastDataRepository.Setup(r => r.GetDataRepository<IPostRepository>()).Returns(postRepository.Object);

            var sut = mocker.Create<PostService>();

            var request = new PostRequest();
            request.FileId = 5;
            request.FileName = "abc.xlsx";
            request.Audiences = new List<int> { 1 };
            request.PlaybackType = ProposalEnums.ProposalPlaybackType.Live;

            Assert.That(() => sut.EditPost(request), Throws.Exception.With.Message.EqualTo(string.Format(PostService.DuplicateFileErrorMessage, request.FileName)));
        }
    }
}
