using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using crossblog.Controllers;
using crossblog.Domain;
using crossblog.Model;
using crossblog.Repositories;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace crossblog.tests.Controllers
{
    public class ArticlesControllerTests
    {
        private ArticlesController _articlesController;

        private Mock<IArticleRepository> _articleRepositoryMock = new Mock<IArticleRepository>();

        public ArticlesControllerTests()
        {
            _articlesController = new ArticlesController(_articleRepositoryMock.Object);
        }

        [Fact]
        public async Task Search_ReturnsEmptyList()
        {
            // Arrange
            var articleDbSetMock = Builder<Article>.CreateListOfSize(3).Build().ToAsyncDbSetMock();
            _articleRepositoryMock.Setup(m => m.Query()).Returns(articleDbSetMock.Object);

            // Act
            var result = await _articlesController.Search("Invalid");

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as ArticleListModel;
            Assert.NotNull(content);

            Assert.Empty(content.Articles);
        }

        [Fact]
        public async Task Search_ReturnsList()
        {
            // Arrange
            var articleDbSetMock = Builder<Article>.CreateListOfSize(3).Build().ToAsyncDbSetMock();
            _articleRepositoryMock.Setup(m => m.Query()).Returns(articleDbSetMock.Object);

            // Act
            var result = await _articlesController.Search("Title");

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as ArticleListModel;
            Assert.NotNull(content);

            Assert.Equal(3, content.Articles.Count());
        }

        [Fact]
        public async Task Get_NotFound()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(null));

            // Act
            var result = await _articlesController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task Get_ReturnsItem()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(Builder<Article>.CreateNew().Build()));

            // Act
            var result = await _articlesController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as ArticleModel;
            Assert.NotNull(content);

            Assert.Equal("Title1", content.Title);
        }

        [Fact]
        public async Task Post_Article_InvalidState()
        {
            // Arrange
            var mockArticleModel = new ArticleModel { Title = "mock article" };
            _articlesController.ModelState.AddModelError("Description", "This field is required");
            // Act
            var result = await _articlesController.Post(mockArticleModel);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(new SerializableError(_articlesController.ModelState), actionResult.Value);
        }

        [Fact]
        public async Task CommentPost_validState()
        {
            // Arrange
            // Act
            var result = await _articlesController.Post(Builder<ArticleModel>.CreateNew().Build());
            // Assert
            var objectResult = result as CreatedResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as Article;
            Assert.NotNull(content);

        }

        [Fact]
        public async Task Put_Article_InvalidState()
        {
            // Arrange
            var mockArticleModel = new ArticleModel { Title = "mock article" };
            _articlesController.ModelState.AddModelError("Description", "This field is required");
            // Act
            var result = await _articlesController.Put(0, mockArticleModel);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(new SerializableError(_articlesController.ModelState), actionResult.Value);
        }
    }
}
