using System.Linq.Expressions;
using BloggingPlatform.Business.Models.BaseModels;
using BloggingPlatform.Business.Models.Requests;
using BloggingPlatform.Business.Services.Providers;
using BloggingPlatform.Data.Entities;
using BloggingPlatform.Data.Repositories.Interface;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BloggingPlatForm.UnitTest
{
    public class BlogpostServiceTests
    {
        private readonly BlogpostService _sut;
        private readonly IGenericRepository<BlogPost> _blogPostRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly ILogger<BlogpostService> _logger;

        public BlogpostServiceTests()
        {
            _logger = Substitute.For<ILogger<BlogpostService>>();
            _blogPostRepository = Substitute.For<IGenericRepository<BlogPost>>();
            _userRepository = Substitute.For<IGenericRepository<User>>();
            _sut = new BlogpostService(_blogPostRepository, _logger, _userRepository);
        }

        #region CreateBlogPostAsync Tests

        [Fact]
        public async Task CreateBlogPostAsync_Should_Return_Ok_When_BlogPost_Is_Created_Successfully()
        {
            // Arrange
            var userId = "1";
            var request = new BlogPostRequest { Title = "Test Title", Content = "Test Content" };
            var user = new User { Id = userId, FullName = "Test User", Password = "Test Password", PhoneNumber = "0274810934"};
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _blogPostRepository.AddAsync(Arg.Any<BlogPost>()).Returns(true);

            // Act
            var result = await _sut.CreateBlogPostAsync(userId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
                result.Data.Should().NotBeNull();
                result.Data.Title.Should().Be("Test Title");
                result.Data.Content.Should().Be("Test Content");
            }
        }

        [Fact]
        public async Task CreateBlogPostAsync_Should_Return_NotFound_When_User_Is_Not_Found()
        {
            // Arrange
            var userId = "1";
            var request = new BlogPostRequest { Title = "Test Title", Content = "Test Content" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns((User)null);

            // Act
            var result = await _sut.CreateBlogPostAsync(userId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("User not found");
            }
        }

        [Fact]
        public async Task CreateBlogPostAsync_Should_Return_BadRequest_When_Request_Is_Invalid()
        {
            // Arrange
            var userId = "1";
            var request = new BlogPostRequest { Title = "", Content = "" };

            // Act
            var result = await _sut.CreateBlogPostAsync(userId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(400);
                result.Message.Should().Be("Title and content are required");
            }
        }

        [Fact]
        public async Task CreateBlogPostAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var userId = "1";
            var request = new BlogPostRequest { Title = "Test Title", Content = "Test Content" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(Task.FromException<User>(new Exception("Exception occurred")));

            // Act
            var result = await _sut.CreateBlogPostAsync(userId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }

        #endregion

        #region UpdateBlogPostAsync Tests

        [Fact]
        public async Task UpdateBlogPostAsync_Should_Return_Ok_When_BlogPost_Is_Updated_Successfully()
        {
            // Arrange
            var postId = "1";
            var request = new UpdateBlogpostRequest { Title = "Updated Title", Content = "Updated Content" };
            var blogPost = new BlogPost { Id = postId, IsActive = true, Title = "Original Title", Content = "Original Content", Author = "Author", CreatedAt = DateTime.Now };
            _blogPostRepository.FindAsync(Arg.Any<Expression<Func<BlogPost, bool>>>()).Returns(blogPost);
            _blogPostRepository.UpdateAsync(blogPost).Returns(true);

            // Act
            var result = await _sut.UpdateBlogPostAsync(postId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
                result.Data.Should().NotBeNull();
                result.Data.Title.Should().Be("Updated Title");
                result.Data.Content.Should().Be("Updated Content");
            }
        }

        [Fact]
        public async Task UpdateBlogPostAsync_Should_Return_NotFound_When_BlogPost_Is_Not_Found()
        {
            // Arrange
            var postId = "1";
            var request = new UpdateBlogpostRequest { Title = "Updated Title", Content = "Updated Content" };
            _blogPostRepository.FindAsync(Arg.Any<Expression<Func<BlogPost, bool>>>()).Returns((BlogPost)null);

            // Act
            var result = await _sut.UpdateBlogPostAsync(postId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("Blog post not found");
            }
        }

        [Fact]
        public async Task UpdateBlogPostAsync_Should_Return_BadRequest_When_Request_Is_Invalid()
        {
            // Arrange
            var postId = "1";
            var request = new UpdateBlogpostRequest { Title = "", Content = "" };

            // Act
            var result = await _sut.UpdateBlogPostAsync(postId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(400);
                result.Message.Should().Be("Title and content are required");
            }
        }

        [Fact]
        public async Task UpdateBlogPostAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var postId = "1";
            var request = new UpdateBlogpostRequest { Title = "Updated Title", Content = "Updated Content" };
            _blogPostRepository.FindAsync(Arg.Any<Expression<Func<BlogPost, bool>>>()).Returns(Task.FromException<BlogPost>(new Exception("Exception occurred")));

            // Act
            var result = await _sut.UpdateBlogPostAsync(postId, request);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }

        #endregion

        #region DeleteBlogPostAsync Tests

        [Fact]
        public async Task DeleteBlogPostAsync_Should_Return_Ok_When_BlogPost_Is_Deleted_Successfully()
        {
            // Arrange
            var postId = "1";
            var blogPost = new BlogPost { Id = postId, IsActive = true, Title = "Test Title", Content = "Test Content", Author = "Author", CreatedAt = DateTime.Now };
            _blogPostRepository.FindAsync(Arg.Any<Expression<Func<BlogPost, bool>>>()).Returns(blogPost);
            _blogPostRepository.SoftDeleteAsync(postId).Returns(true);

            // Act
            var result = await _sut.DeleteBlogPostAsync(postId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
                result.Data.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task DeleteBlogPostAsync_Should_Return_NotFound_When_BlogPost_Is_Not_Found()
        {
            // Arrange
            var postId = "1";
            _blogPostRepository.FindAsync(Arg.Any<Expression<Func<BlogPost, bool>>>()).Returns((BlogPost)null);

            // Act
            var result = await _sut.DeleteBlogPostAsync(postId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("Blog post not found");
            }
        }

        [Fact]
        public async Task DeleteBlogPostAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var postId = "1";
            _blogPostRepository.FindAsync(Arg.Any<Expression<Func<BlogPost, bool>>>()).Returns(Task.FromException<BlogPost>(new Exception("Exception occurred")));

            // Act
            var result = await _sut.DeleteBlogPostAsync(postId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }

        #endregion

        #region GetBlogPostByIdAsync Tests

        // [Fact]
        // public async Task GetBlogPostByIdAsync_Should_Return_Ok_When_BlogPost_Is_Found()
        // {
        //     // Arrange
        //     var postId = "1";
        //     var blogPost = new BlogPost
        //     {
        //         Id = postId,
        //         IsActive = true,
        //         Comments = new List<Comment>
        //         {
        //             new Comment
        //             {
        //                 Id = "1",
        //                 IsActive = true,
        //                 Content = "Test Comment",
        //                 Commenter = "Test Commenter",
        //             }
        //         },
        //         Title = "Test Title",
        //         Content = "Test Content",
        //         Author = "Author",
        //     };
        //     var mockBlogPosts = new List<BlogPost> { blogPost }.AsQueryable().AsQueryable();
        //     _blogPostRepository.AsQueryable().Returns(mockBlogPosts);
        //
        //     // Act
        //     var result = await _sut.GetBlogPostByIdAsync(postId);
        //
        //     // Assert
        //     using (new AssertionScope())
        //     {
        //         result.Should().NotBeNull();
        //         result.Code.Should().Be(200);
        //         result.Data.Should().NotBeNull();
        //         result.Data.Id.Should().Be(postId);
        //     }
        // }

        // [Fact]
        // public async Task GetBlogPostByIdAsync_Should_Return_NotFound_When_BlogPost_Is_Not_Found()
        // {
        //     // Arrange
        //     var postId = "1";
        //     var mockBlogPosts = new List<BlogPost>().AsQueryable().AsQueryable();
        //     _blogPostRepository.AsQueryable().Returns(mockBlogPosts);
        //
        //     // Act
        //     var result = await _sut.GetBlogPostByIdAsync(postId);
        //
        //     // Assert
        //     using (new AssertionScope())
        //     {
        //         result.Should().NotBeNull();
        //         result.Code.Should().Be(404);
        //         result.Message.Should().Be("Blog post not found");
        //     }
        // }

        [Fact]
        public async Task GetBlogPostByIdAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var postId = "1";
            _blogPostRepository.AsQueryable().Returns(_ => throw new Exception("Exception occurred"));

            // Act
            var result = await _sut.GetBlogPostByIdAsync(postId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }

        #endregion

        #region GetAllBlogPostsAsync Tests

        // [Fact]
        // public async Task GetAllBlogPostsAsync_Should_Return_Ok_When_BlogPosts_Are_Found()
        // {
        //     // Arrange
        //     var filter = new BaseFilter { PageNumber = 1, PageSize = 10, Search = "Test" };
        //     var blogPosts = new List<BlogPost>
        //     {
        //         new BlogPost
        //         {
        //             Id = "1",
        //             Title = "Test Title",
        //             Content = "Test Content",
        //             IsActive = true,
        //             CreatedAt = DateTime.Now,
        //             Comments = new List<Comment>
        //             {
        //                 new Comment
        //                 {
        //                     Id = "1",
        //                     IsActive = true,
        //                     Content = "Test Comment",
        //                     Commenter = "Test Commenter",
        //                 }
        //             },
        //             Author = "Author"
        //         }
        //     }.AsQueryable().AsQueryable();
        //
        //     _blogPostRepository.AsQueryable().Returns(blogPosts);
        //
        //     // Act
        //     var result = await _sut.GetAllBlogPostsAsync(filter);
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

        // [Fact]
        // public async Task GetAllBlogPostsAsync_Should_Return_Ok_When_No_BlogPosts_Are_Found()
        // {
        //     // Arrange
        //     var filter = new BaseFilter { PageNumber = 1, PageSize = 10, Search = "Test" };
        //     var blogPosts = new List<BlogPost>().AsQueryable().AsQueryable();
        //
        //     _blogPostRepository.AsQueryable().Returns(blogPosts);
        //
        //     // Act
        //     var result = await _sut.GetAllBlogPostsAsync(filter);
        //
        //     // Assert
        //     using (new AssertionScope())
        //     {
        //         result.Should().NotBeNull();
        //         result.Code.Should().Be(200);
        //         result.Data.Should().NotBeNull();
        //         result.Data.Payload.Should().BeEmpty();
        //     }
        // }

        [Fact]
        public async Task GetAllBlogPostsAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var filter = new BaseFilter { PageNumber = 1, PageSize = 10, Search = "Test" };
            _blogPostRepository.AsQueryable().Returns(_ => throw new Exception("Exception occurred"));

            // Act
            var result = await _sut.GetAllBlogPostsAsync(filter);

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