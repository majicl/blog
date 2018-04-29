using crossblog.Controllers;
using crossblog.Domain;
using crossblog.Model;
using crossblog.Repositories;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
namespace crossblog.tests.Controllers
{
    public class CommentsControllerTests
    {
        private CommentsController _commentsController;
        private Mock<ICommentRepository> _commentsRepositoryMock = new Mock<ICommentRepository>();
        private Mock<IArticleRepository> _articleRepositoryMock = new Mock<IArticleRepository>();
        public CommentsControllerTests()
        {
            _commentsController = new CommentsController(_articleRepositoryMock.Object, _commentsRepositoryMock.Object);
        }

        [Fact]
        public async Task Get_NotFound()
        {
            // Arrange
            _commentsRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Comment>(null));

            // Act
            var result = await _commentsController.Get(1);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task ReturnJustReletiveComments()
        {
            // Arrange
            var commentDbSetMock = Builder<Comment>.CreateListOfSize(30).Build().ToAsyncDbSetMock();
            var maxItem = 0;
            foreach (var item in commentDbSetMock.Object)
            {
                if (maxItem >= 10)
                {
                    break;
                }
                item.ArticleId = 2;
                maxItem++;
            }
            _commentsRepositoryMock.Setup(m => m.Query()).Returns(commentDbSetMock.Object);
            _articleRepositoryMock.Setup(m => m.GetAsync(2)).Returns(Task.FromResult(Builder<Article>.CreateNew().Build()));
            // Act
            var result = await _commentsController.Get(2);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as CommentListModel;
            Assert.NotNull(content);

            Assert.Equal(10, content.Comments.Count());
        }

        [Fact]
        public async Task GetSpeceficComment()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult(Builder<Article>.CreateNew().Build()));
            var commentDbSetMock = Builder<Comment>.CreateListOfSize(10).Build().ToAsyncDbSetMock();
            var maxItem = 0;
            foreach (var item in commentDbSetMock.Object)
            {
                if (maxItem >= 10)
                {
                    break;
                }
                item.ArticleId = 1;
                maxItem++;
            }
            commentDbSetMock.Object.First().Id = 2;
            _commentsRepositoryMock.Setup(m => m.Query()).Returns(commentDbSetMock.Object);
            // Act
            var result = await _commentsController.Get(3, 2);

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);

            var resultComment = await _commentsController.Get(1, 0);
            var objectResultComment = resultComment as NotFoundResult;
            Assert.NotNull(objectResultComment);

            var resultok = await _commentsController.Get(1, 2);
            var objectResultok = resultok as OkObjectResult;
            Assert.NotNull(objectResultok);

            var content = objectResultok.Value as CommentModel;
            Assert.NotNull(content);

            Assert.Equal(2, content.Id);
        }

        [Fact]
        public async Task Get_CommentPost_NotFound()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult<Article>(null));
            // Act
            var result = await _commentsController.Post(1, new CommentModel { });

            // Assert
            Assert.NotNull(result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull(objectResult);
        }

        [Fact]
        public async Task CommentPost_InvalidState()
        {
            // Arrange
            var mockCommentModel = new CommentModel { Title = "mock comment" };
            _commentsController.ModelState.AddModelError("Description", "This field is required");
            // Act
            var result = await _commentsController.Post(1, mockCommentModel);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(new SerializableError(_commentsController.ModelState), actionResult.Value);
        }

        [Fact]
        public async Task CommentPost_validState()
        {
            // Arrange
            _articleRepositoryMock.Setup(m => m.GetAsync(1)).Returns(Task.FromResult(Builder<Article>.CreateNew().Build()));
            // Act
            var result = await _commentsController.Post(1, Builder<CommentModel>.CreateNew().Build());
            // Assert
            var objectResult = result as CreatedResult;
            Assert.NotNull(objectResult);

            var content = objectResult.Value as CommentModel;
            Assert.NotNull(content);

        }
    }
}
