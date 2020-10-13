using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    [Category("short_running")]
    public class PostingBookServiceTests
    {
        private Mock<IRatingForecastService> _ratingForecastServiceMock;

        [SetUp]
        public void Init()
        {
            _ratingForecastServiceMock = new Mock<IRatingForecastService>();
        }

        [Test]
        public void GetHUTBooksThrowsWhenShareBookNotFound()
        {
            //Arrange
            _ratingForecastServiceMock.Setup(x => x.GetMediaMonthCrunchStatuses())
                .Returns(new List<MediaMonthCrunchStatus>());

            var postingBookService = new PostingBookService(_ratingForecastServiceMock.Object, null, null);

            //Act & Assert
            Assert.Throws<ArgumentException>(() => postingBookService.GetHUTBooks(122));
        }


        [Test]
        public void GetHUTBooksReturnsEmptyListWhenNoAvailableHUTBooks()
        {
            //Arrange
            var sequenceGenerator = new SequentialGenerator<int> { Direction = GeneratorDirection.Ascending, Increment = 1 };
            sequenceGenerator.StartingWith(1);

            var mediaMonthCrunchStatuses = Builder<MediaMonthCrunchStatus>.CreateListOfSize(5)
                .All()
                .WithFactory(() =>
                {
                    var mediaMonth = Builder<MediaMonth>.CreateNew()
                    .With(z => z.Id = sequenceGenerator.Generate())
                    .Build();

                    var mediaMonthCrunchStatus = new MediaMonthCrunchStatus(
                        Builder<RatingsForecastStatus>.CreateNew()
                        .With(y => y.MediaMonth = mediaMonth)
                        .With(y => y.UniverseMarkets = 1)
                        .Build()
                        , 1);

                    return mediaMonthCrunchStatus;
                })
                .Build()
                .ToList();

            _ratingForecastServiceMock.Setup(x => x.GetMediaMonthCrunchStatuses())
                .Returns(mediaMonthCrunchStatuses);

            var postingBookService = new PostingBookService(_ratingForecastServiceMock.Object, null, null);

            //Act
            var result = postingBookService.GetHUTBooks(3); //3 - 12 here should give us an id that does not match

            //Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetHUTBooksReturnsListOf3WhenAvailableHUTBooks()
        {
            //Arrange
            var sequenceGenerator = new SequentialGenerator<int> { Direction = GeneratorDirection.Ascending, Increment = 1 };
            sequenceGenerator.StartingWith(1);

            var mediaMonthCrunchStatuses = Builder<MediaMonthCrunchStatus>.CreateListOfSize(15)
                .All()
                .WithFactory(() =>
                {
                    var mediaMonth = Builder<MediaMonth>.CreateNew()
                    .With(x => x.Id = sequenceGenerator.Generate())
                    .With(x => x.StartDate = DateTime.Now.AddMonths(x.Id))
                    .Build();

                    var mediaMonthCrunchStatus = new MediaMonthCrunchStatus(
                        Builder<RatingsForecastStatus>.CreateNew()
                        .With(y => y.MediaMonth = mediaMonth)
                        .With(y => y.UniverseMarkets = 1)
                        .Build()
                        , 1);

                    return mediaMonthCrunchStatus;
                })
                .Build()
                .ToList();

            _ratingForecastServiceMock.Setup(x => x.GetMediaMonthCrunchStatuses())
                .Returns(mediaMonthCrunchStatuses);

            var postingBookService = new PostingBookService(_ratingForecastServiceMock.Object, null, null);

            //Act
            var result = postingBookService.GetHUTBooks(13);

            //Assert
            //There should be 12 with a start date less than the share book since dates and ids are sequential incrementing for the fake data
            //15 items on the list sharebook is item 13. There are 12 items than are less. 
            Assert.AreEqual(12, result.Count);

            //The default one which should be the first on the result list should be id 1.  13 - 12 to go back one year
            Assert.AreEqual(1, result.First().Id);

        }
    }
}
