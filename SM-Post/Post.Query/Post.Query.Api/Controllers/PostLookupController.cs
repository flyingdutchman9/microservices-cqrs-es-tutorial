using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Post.Common.DTOs;
using Post.Query.Api.DTOs;
using Post.Query.Api.Queries;
using Post.Query.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Post.Query.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PostLookupController : ControllerBase
    {
        private readonly IQueryDispatcher<PostEntity> _dispatcher;
        private readonly ILogger<PostLookupController> _logger;

        public PostLookupController(IQueryDispatcher<PostEntity> dispatcher, ILogger<PostLookupController> logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllPostsAsync()
        {
            try
            {
                var posts = await _dispatcher.SendAsync(new FindAllPostsQuery());
                return NormalResponse(posts);
            }
            catch (System.Exception ex)
            {
                return ErrorResponse(ex, "Error while processing request to retrieve all posts!");
            }
        }

        [HttpGet("{postId}")]
        public async Task<ActionResult> GetByPostIdAsync([FromRoute] Guid postId)
        {
            try
            {
                var posts = await _dispatcher.SendAsync(new FindPostByIdQuery { Id = postId });
                return NormalResponse(posts);
            }
            catch (System.Exception ex)
            {
                return ErrorResponse(ex, "Error while processing request to retrieve post by id!");
            }
        }

        [HttpGet("author/{author}")]
        public async Task<ActionResult> GetByAuthorAsync([FromRoute] string author)
        {
            try
            {
                var posts = await _dispatcher.SendAsync(new FindPostsByAuthorQuery { Author = author });
                return NormalResponse(posts);
            }
            catch (System.Exception ex)
            {
                return ErrorResponse(ex, "Error while processing request to retrieve posts by author!");
            }
        }

        [HttpGet("withComments")]
        public async Task<ActionResult> GetByCommentsAsync()
        {
            try
            {
                var posts = await _dispatcher.SendAsync(new FindPostsWithCommentsQuery());
                return NormalResponse(posts);
            }
            catch (System.Exception ex)
            {
                return ErrorResponse(ex, "Error while processing request to retrieve posts with comments!");
            }
        }

        [HttpGet("withLikes/{numberOfLikes}")]
        public async Task<ActionResult> GetWithLikesAsync([FromRoute]int numberOfLikes)
        {
            try
            {
                var posts = await _dispatcher.SendAsync(new FindPostsWithLikesQuery { NumberOfLikes = numberOfLikes });
                return NormalResponse(posts);
            }
            catch (System.Exception ex)
            {
                return ErrorResponse(ex, "Error while processing request to retrieve posts with number of likes!");
            }
        }

        private ActionResult NormalResponse(List<PostEntity> posts)
        {
            if (posts == null || !posts.Any())
            {
                return NoContent();
            }

            var count = posts.Count;

            return Ok(new PostLookupResponse
            {
                Posts = posts,
                Message = $"Successfully returned {count} post(s)"
            });
        }

        private ActionResult ErrorResponse(Exception ex, string SAFE_ERROR_MSG)
        {
            _logger.LogError(ex, SAFE_ERROR_MSG);

            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MSG
            });
        }
    }
}
