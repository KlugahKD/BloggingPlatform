using System.Linq.Expressions;
using BloggingPlatform.Business.Models.BaseModels;
using BloggingPlatform.Business.Models.Requests;
using BloggingPlatform.Business.Services.Providers;
using BloggingPlatform.Data.Entities;
using BloggingPlatform.Data.Repositories.Interface;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using MockQueryable;
using NSubstitute;

namespace BloggingPlatForm.UnitTest
{
    public class CommentServiceTests
    {
        private readonly CommentService _sut;
        private readonly IGenericRepository<Comment> _commentRepository;
        private readonly IGenericRepository<BlogPost> _blogPostRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly ILogger<CommentService> _logger;

        public CommentServiceTests()
        {
            _logger = Substitute.For<ILogger<CommentService>>();
            _commentRepository = Substitute.For<IGenericRepository<Comment>>();
            _blogPostRepository = Substitute.For<IGenericRepository<BlogPost>>();
            _userRepository = Substitute.For<IGenericRepository<User>>();
            _sut = new CommentService(_commentRepository, _blogPostRepository, _userRepository, _logger);
        }

        #region CreateCommentAsync Tests

        [Fact]
        public async Task CreateCommentAsync_Should_Return_Ok_When_Comment_Is_Created_Successfully()
        {
            // Arrange
            var userId = "1";
            var request = new CommentRequest { PostId = "1", Content = "Test Content" };
            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                PhoneNumber = "0274810934",
                Password = "Test Password",
            };
            var blogPost = new BlogPost
            {
                Id = request.PostId,
                IsActive = true,
                Title = null,
                Content = null,
                Author = null
            };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _blogPostRepository.FindAsync(Arg.Any<Expression<Func<BlogPost, bool>>>()).Returns(blogPost);
            _commentRepository.AddAsync(Arg.Any<Comment>()).Returns(true);

            // Act
            var result = await _sut.CreateCommentAsync(request, userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
                result.Data.Should().NotBeNull();
                result.Data.Content.Should().Be("Test Content");
            }
        }

        [Fact]
        public async Task CreateCommentAsync_Should_Return_NotFound_When_User_Is_Not_Found()
        {
            // Arrange
            var userId = "1";
            var request = new CommentRequest { PostId = "1", Content = "Test Content" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns((User)null);

            // Act
            var result = await _sut.CreateCommentAsync(request, userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("User not found");
            }
        }

        [Fact]
        public async Task CreateCommentAsync_Should_Return_NotFound_When_BlogPost_Is_Not_Found()
        {
            // Arrange
            var userId = "1";
            var request = new CommentRequest { PostId = "1", Content = "Test Content" };
            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                PhoneNumber = "0274810934",
                Password = "Test Password",
            };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _blogPostRepository.FindAsync(Arg.Any<Expression<Func<BlogPost, bool>>>()).Returns((BlogPost)null);

            // Act
            var result = await _sut.CreateCommentAsync(request, userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("Blog post not found");
            }
        }

        [Fact]
        public async Task CreateCommentAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var userId = "1";
            var request = new CommentRequest { PostId = "1", Content = "Test Content" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(Task.FromException<User>(new Exception("Exception occurred")));

            // Act
            var result = await _sut.CreateCommentAsync(request, userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }

        #endregion

        #region UpdateCommentAsync Tests

        [Fact]
        public async Task UpdateCommentAsync_Should_Return_Ok_When_Comment_Is_Updated_Successfully()
        {
            // Arrange
            var commentId = "1";
            var userId = "1";
            var request = new UpdateCommentRequest { Content = "Updated Content" };
            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                PhoneNumber = "0274810934",
                Password = "Test Password",
            };
            var comment = new Comment { Id = commentId, IsActive = true, Commenter = user.FullName, Content = "Original Content" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _commentRepository.FindAsync(Arg.Any<Expression<Func<Comment, bool>>>()).Returns(comment);
            _commentRepository.UpdateAsync(comment).Returns(true);

            // Act
            var result = await _sut.UpdateCommentAsync(commentId, userId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
                result.Data.Should().NotBeNull();
                result.Data.Content.Should().Be("Updated Content");
            }
        }

        [Fact]
        public async Task UpdateCommentAsync_Should_Return_NotFound_When_User_Is_Not_Found()
        {
            // Arrange
            var commentId = "1";
            var userId = "1";
            var request = new UpdateCommentRequest { Content = "Updated Content" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(null as User);

            // Act
            var result = await _sut.UpdateCommentAsync(commentId, userId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("User not found");
            }
        }

        [Fact]
        public async Task UpdateCommentAsync_Should_Return_NotFound_When_Comment_Is_Not_Found()
        {
            // Arrange
            var commentId = "1";
            var userId = "1";
            var request = new UpdateCommentRequest { Content = "Updated Content" };
            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                PhoneNumber = "0274810934",
                Password = "Test Password",
            };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _commentRepository.FindAsync(Arg.Any<Expression<Func<Comment, bool>>>()).Returns((Comment)null);

            // Act
            var result = await _sut.UpdateCommentAsync(commentId, userId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("Comment not found");
            }
        }

        // [Fact]
        // public async Task UpdateCommentAsync_Should_Return_Forbidden_When_User_Is_Not_Authorized()
        // {
        //     // Arrange
        //     var commentId = "1";
        //     var userId = "1";
        //     var request = new UpdateCommentRequest { Content = "Updated Content" };
        //     var user = new User
        //     {
        //         Id = userId,
        //         FullName = "Test User",
        //         PhoneNumber = "0274810934",
        //         Password = "Test Password",
        //     };
        //     var comment = new Comment { Id = commentId, IsActive = true, Commenter = "Another User", Content = "Original Content" };
        //     _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
        //     _commentRepository.FindAsync(Arg.Any<Expression<Func<Comment, bool>>>()).Returns(comment);
        //
        //     // Act
        //     var result = await _sut.UpdateCommentAsync(commentId, userId, request);
        //
        //     // Assert
        //     using (new AssertionScope())
        //     {
        //         result.Should().NotBeNull();
        //         result.Code.Should().Be(403);
        //         result.Message.Should().Be("Comment was not made by the user, You cant update it");
        //     }
        // }

        [Fact]
        public async Task UpdateCommentAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var commentId = "1";
            var userId = "1";
            var request = new UpdateCommentRequest { Content = "Updated Content" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(Task.FromException<User>(new Exception("Exception occurred")));

            // Act
            var result = await _sut.UpdateCommentAsync(commentId, userId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }

        #endregion

        #region DeleteCommentAsync Tests

        [Fact]
        public async Task DeleteCommentAsync_Should_Return_Ok_When_Comment_Is_Deleted_Successfully()
        {
            // Arrange
            var commentId = "1";
            var userId = "1";
            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                Role = "Admin",
                PhoneNumber = "0274810934",
                Password = "Test Password",
            };
            var comment = new Comment
            {
                Id = commentId,
                IsActive = true,
                Commenter = user.FullName,
                Content = "Test Content"
            };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _commentRepository.FindAsync(Arg.Any<Expression<Func<Comment, bool>>>()).Returns(comment);
            _commentRepository.SoftDeleteAsync(commentId).Returns(true);

            // Act
            var result = await _sut.DeleteCommentAsync(commentId, userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
                result.Data.Should().NotBeNull();
            }
        }

        // [Fact]
        // public async Task DeleteCommentAsync_Should_Return_NotFound_When_User_Is_Not_Found()
        // {
        //     // Arrange
        //     var commentId = "1";
        //     var userId = "1";
        //     _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(null as User);
        //
        //     // Act
        //     var result = await _sut.DeleteCommentAsync(commentId, userId);
        //
        //     // Assert
        //     using (new AssertionScope())
        //     {
        //         result.Should().NotBeNull();
        //         result.Code.Should().Be(404);
        //         result.Message.Should().Be("User not found");
        //     }
        // }

        [Fact]
        public async Task DeleteCommentAsync_Should_Return_NotFound_When_Comment_Is_Not_Found()
        {
            // Arrange
            var commentId = "1";
            var userId = "1";
            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                PhoneNumber = "0274810934",
                Password = "Test Password",
            };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _commentRepository.FindAsync(Arg.Any<Expression<Func<Comment, bool>>>()).Returns((Comment)null);

            // Act
            var result = await _sut.DeleteCommentAsync(commentId, userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("Comment not found");
            }
        }

        [Fact]
        public async Task DeleteCommentAsync_Should_Return_Forbidden_When_User_Is_Not_Authorized()
        {
            // Arrange
            var commentId = "1";
            var userId = "1";
            var user = new User
            {
                Id = userId,
                FullName = "Test User",
                PhoneNumber = "0274810934",
                Password = "Test Password",
            };
            var comment = new Comment
            {
                Id = commentId,
                IsActive = true,
                Commenter = "Another User",
                Content = null
            };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _commentRepository.FindAsync(Arg.Any<Expression<Func<Comment, bool>>>()).Returns(comment);

            // Act
            var result = await _sut.DeleteCommentAsync(commentId, userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(403);
                result.Message.Should().Be("User is not authorized to delete this comment");
            }
        }

        // [Fact]
        // public async Task DeleteCommentAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        // {
        //     // Arrange
        //     var commentId = "1";
        //     var userId = "1";
        //     _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(Task.FromException<User>(new Exception("Exception occurred")));
        //
        //     // Act
        //     var result = await _sut.DeleteCommentAsync(commentId, userId);
        //
        //     // Assert
        //     using (new AssertionScope())
        //     {
        //         result.Should().NotBeNull();
        //         result.Code.Should().Be(500);
        //         result.Message.Should().Be("Something Bad Happened. Please try again later.");
        //     }
        // }

        #endregion

        #region GetCommentByIdAsync Tests

        [Fact]
        public async Task GetCommentByIdAsync_Should_Return_Ok_When_Comment_Is_Found()
        {
            // Arrange
            var commentId = "1";
            var comment = new Comment
            {
                Id = commentId,
                IsActive = true,
                Content = "Test Content",
                Commenter = null
            };
            _commentRepository.FindAsync(Arg.Any<Expression<Func<Comment, bool>>>()).Returns(comment);

            // Act
            var result = await _sut.GetCommentByIdAsync(commentId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
                result.Data.Should().NotBeNull();
                result.Data.Content.Should().Be("Test Content");
            }
        }

        [Fact]
        public async Task GetCommentByIdAsync_Should_Return_NotFound_When_Comment_Is_Not_Found()
        {
            // Arrange
            var commentId = "1";
            _commentRepository.FindAsync(Arg.Any<Expression<Func<Comment, bool>>>()).Returns((Comment)null);

            // Act
            var result = await _sut.GetCommentByIdAsync(commentId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("Comment not found");
            }
        }

        [Fact]
        public async Task GetCommentByIdAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var commentId = "1";
            _commentRepository.FindAsync(Arg.Any<Expression<Func<Comment, bool>>>()).Returns(Task.FromException<Comment>(new Exception("Exception occurred")));

            // Act
            var result = await _sut.GetCommentByIdAsync(commentId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }

        #endregion

        #region GetAllCommentsByBlogPostIdAsync Tests

        // [Fact]
        // public async Task GetAllCommentsByBlogPostIdAsync_Should_Return_Ok_When_Comments_Are_Found()
        // {
        //     // Arrange
        //     var blogPostId = "1";
        //     var filter = new BaseFilter { PageNumber = 1, PageSize = 10, Search = "Test" };
        //     var blogPost = new BlogPost
        //     {
        //         Id = blogPostId,
        //         IsActive = true,
        //         Title = null,
        //         Content = null,
        //         Author = null
        //     };
        //     var comments = new List<Comment>
        //     {
        //         new Comment { Id = "1", Content = "Test Content", IsActive = true, CreatedAt = DateTime.UtcNow, Commenter = "Test User" }
        //     }.AsQueryable().BuildMock();
        //     _blogPostRepository.ExistsAsync(Arg.Any<Expression<Func<BlogPost, bool>>>()).Returns(true);
        //     _commentRepository.AsQueryable().Returns(comments);
        //
        //     // Act
        //     var result = await _sut.GetAllCommentsByBlogPostIdAsync(blogPostId, filter);
        //
        //     // Assert
        //     using (new AssertionScope())
        //     {
        //         result.Should().NotBeNull();
        //         result.Code.Should().Be(200);
        //         result.Data.Should().NotBeNull();
        //         result.Data.Payload.Should().HaveCount(1);
        //     }
        // }

        [Fact]
        public async Task GetAllCommentsByBlogPostIdAsync_Should_Return_NotFound_When_BlogPost_Is_Not_Found()
        {
            // Arrange
            var blogPostId = "1";
            var filter = new BaseFilter { PageNumber = 1, PageSize = 10, Search = "Test" };
            _blogPostRepository.ExistsAsync(Arg.Any<Expression<Func<BlogPost, bool>>>()).Returns(false);

            // Act
            var result = await _sut.GetAllCommentsByBlogPostIdAsync(blogPostId, filter);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("Blog post not found");
            }
        }

        [Fact]
        public async Task GetAllCommentsByBlogPostIdAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var blogPostId = "1";
            var filter = new BaseFilter { PageNumber = 1, PageSize = 10, Search = "Test" };
            _blogPostRepository.ExistsAsync(Arg.Any<Expression<Func<BlogPost, bool>>>()).Returns(Task.FromException<bool>(new Exception("Exception occurred")));

            // Act
            var result = await _sut.GetAllCommentsByBlogPostIdAsync(blogPostId, filter);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }

        #endregion
    }
}