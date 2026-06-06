using AIInterviewPlatform.Application.DTOs.TargetJob;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TargetJobsController : ControllerBase
    {
        private readonly ITargetJobService _targetJobService;

        public TargetJobsController(
            ITargetJobService targetJobService)
        {
            _targetJobService = targetJobService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTargetJob(
            CreateTargetJobRequest request)
        {
            try
            {
                var userId = GetUserId();

                var result =
                    await _targetJobService.CreateTargetJobAsync(
                        userId,
                        request
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyTargetJobs()
        {
            try
            {
                var userId = GetUserId();

                var result =
                    await _targetJobService.GetMyTargetJobsAsync(
                        userId
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTargetJobById(
            long id)
        {
            try
            {
                var userId = GetUserId();

                var result =
                    await _targetJobService.GetTargetJobByIdAsync(
                        userId,
                        id
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTargetJob(
            long id,
            UpdateTargetJobRequest request)
        {
            try
            {
                var userId = GetUserId();

                var result =
                    await _targetJobService.UpdateTargetJobAsync(
                        userId,
                        id,
                        request
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTargetJob(
            long id)
        {
            try
            {
                var userId = GetUserId();

                await _targetJobService.DeleteTargetJobAsync(
                    userId,
                    id
                );

                return Ok(new
                {
                    message = "Target job deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("{id}/job-description")]
        public async Task<IActionResult> AddJobDescription(
            long id,
            CreateJobDescriptionRequest request)
        {
            try
            {
                var userId = GetUserId();

                var result =
                    await _targetJobService.AddJobDescriptionAsync(
                        userId,
                        id,
                        request
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("{id}/job-description")]
        public async Task<IActionResult> GetJobDescription(
            long id)
        {
            try
            {
                var userId = GetUserId();

                var result =
                    await _targetJobService.GetJobDescriptionAsync(
                        userId,
                        id
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        private long GetUserId()
        {
            return long.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!
                    .Value
            );
        }
    }
}