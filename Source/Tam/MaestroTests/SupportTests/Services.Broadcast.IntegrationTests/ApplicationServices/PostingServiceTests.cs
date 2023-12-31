﻿using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using Moq.AutoMock;
using Services.Broadcast.Cache;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class PostServiceTests
    {
        [Test]
        public void SavePostFile_Throws_When_File_Not_Excel()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();

            var request = new PostRequest();
            request.FileName = "abc.notexcel";

            Assert.That(() => sut.SavePost(request), Throws.Exception.With.Message.EqualTo(PostPrePostingService.FileNotExcelErroMessage));
        }

        [Test]
        public void SavePostFile_Throws_When_Audiences_Null()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();

            var request = new PostRequest();
            request.FileName = "abc.xlsx";
            request.Audiences = null;

            Assert.That(() => sut.SavePost(request), Throws.Exception.With.Message.EqualTo(PostPrePostingService.NoAudiencesErrorMessage));
        }

        [Test]
        public void SavePostFile_Throws_When_Invalid_PlaybackType()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();

            var request = new PostRequest();
            request.FileName = "abc.xlsx";
            request.Audiences = new List<int> { 31 };
            request.PlaybackType = (ProposalEnums.ProposalPlaybackType)'a';

            Assert.That(() => sut.SavePost(request), Throws.Exception.With.Message.EqualTo(string.Format(PostPrePostingService.InvalidPlaybackTypeErrorMessage, (char)request.PlaybackType)));
        }

        [Test]
        public void SavePostFile_Throws_When_FileName_Already_Exist()
        {
            var mocker = new AutoMocker();
            var postRepository = mocker.GetMock<IPostPrePostingRepository>();
            postRepository.Setup(r => r.PostExist(It.IsAny<string>())).Returns(true);
            var broadcastDataRepository = mocker.GetMock<IDataRepositoryFactory>();
            broadcastDataRepository.Setup(r => r.GetDataRepository<IPostPrePostingRepository>()).Returns(postRepository.Object);
            var audienceCache = mocker.GetMock<IBroadcastAudiencesCache>();
            var audienceLookups = new List<LookupDto>
            {
                new LookupDto
                {
                    Id = 31,
                },
                new LookupDto
                {
                    Id = 18
                }
            };
            audienceCache.Setup(m => m.GetAllLookups()).Returns(audienceLookups);

            var sut = mocker.CreateInstance<PostPrePostingService>();

            var request = new PostRequest();
            request.FileName = "abc.xlsx";
            request.Audiences = new List<int> { 31 };
            request.PlaybackType = ProposalEnums.ProposalPlaybackType.Live;

            Assert.That(() => sut.SavePost(request), Throws.Exception.With.Message.EqualTo(string.Format(PostPrePostingService.DuplicateFileErrorMessage, request.FileName)));
        }

        [Test]
        public void EditPostFile_Throws_When_Audiences_Null()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();

            var request = new PostRequest();
            request.FileId = 1;
            request.FileName = "abc.xlsx";
            request.Audiences = null;

            Assert.That(() => sut.EditPost(request), Throws.Exception.With.Message.EqualTo(PostPrePostingService.NoAudiencesErrorMessage));
        }

        [Test]
        public void EditPostFile_InvalidAudience()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();

            var request = new PostRequest();
            request.FileId = 1;
            request.FileName = "abc.xlsx";
            request.Audiences = new List<int>(){ 31, -1, 18};

            Assert.That(() => sut.EditPost(request), Throws.Exception.With.Message.EqualTo(PostPrePostingService.InvalidAudienceErrorMessage));
        }

        [Test]
        public void EditPostFile_Throws_When_Invalid_PlaybackType()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();

            var request = new PostRequest();
            request.FileId = 1;
            request.FileName = "abc.xlsx";
            request.Audiences = new List<int> { 31 };
            request.PlaybackType = (ProposalEnums.ProposalPlaybackType)'a';

            Assert.That(() => sut.EditPost(request), Throws.Exception.With.Message.EqualTo(string.Format(PostPrePostingService.InvalidPlaybackTypeErrorMessage, (char)request.PlaybackType)));
        }

        [Test]
        public void EditPostFile_Throws_When_No_Id()
        {
            var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();

            var request = new PostRequest();
            request.FileId = null;

            Assert.That(() => sut.EditPost(request), Throws.Exception.With.Message.EqualTo(PostPrePostingService.MissingId));
        }

        [Test]
        public void Edit_Throws_When_FileName_Already_Exist()
        {
            var mocker = new AutoMocker();
            var postRepository = mocker.GetMock<IPostPrePostingRepository>();

            postRepository.Setup(r => r.PostExist(It.IsAny<string>())).Returns(true);
            var broadcastDataRepository = mocker.GetMock<IDataRepositoryFactory>();

            broadcastDataRepository.Setup(r => r.GetDataRepository<IPostPrePostingRepository>()).Returns(postRepository.Object);

            var audienceCache = mocker.GetMock<IBroadcastAudiencesCache>();
            var audienceLookups = new List<LookupDto>
            {
                new LookupDto
                {
                    Id = 31,
                },
                new LookupDto
                {
                    Id = 18
                }
            };
            audienceCache.Setup(m => m.GetAllLookups()).Returns(audienceLookups);

            var sut = mocker.CreateInstance<PostPrePostingService>();

            var request = new PostRequest();
            request.FileId = 5;
            request.FileName = "abc.xlsx";
            request.Audiences = new List<int> { 31 };
            request.PlaybackType = ProposalEnums.ProposalPlaybackType.Live;

            Assert.That(() => sut.EditPost(request), Throws.Exception.With.Message.EqualTo(string.Format(PostPrePostingService.DuplicateFileErrorMessage, request.FileName)));
        }
    }
}
