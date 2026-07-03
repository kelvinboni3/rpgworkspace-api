using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RpgWorkspace.Application.DTOs.WorkspaceMember;
using RpgWorkspace.Application.Interfaces;

namespace RpgWorkspace.Api.Controllers;

[Authorize]
public class WorkspaceMembersController : ApiController
{
    private readonly IWorkspaceMemberService _workspaceMemberService;

    public WorkspaceMembersController(IWorkspaceMemberService workspaceMemberService)
    {
        _workspaceMemberService = workspaceMemberService;
    }

    /// <summary>Lista os membros de um workspace.</summary>
    [HttpGet("/api/workspaces/{workspaceId:guid}/members")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkspaceMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllByWorkspace(Guid workspaceId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workspaceMemberService.GetAllByWorkspaceAsync(workspaceId, userId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Altera o papel de um membro. Apenas Owner.</summary>
    [HttpPut("/api/workspaces/{workspaceId:guid}/members/{memberId:guid}/role")]
    [ProducesResponseType(typeof(WorkspaceMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(
        Guid workspaceId,
        Guid memberId,
        [FromBody] UpdateWorkspaceMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workspaceMemberService.UpdateRoleAsync(
                workspaceId,
                memberId,
                request,
                userId,
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    /// <summary>Remove um membro do workspace. Apenas Owner.</summary>
    [HttpDelete("/api/workspaces/{workspaceId:guid}/members/{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid workspaceId, Guid memberId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _workspaceMemberService.DeleteAsync(workspaceId, memberId, userId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub")
               ?? throw new InvalidOperationException("User ID claim not found.");

        return Guid.Parse(sub);
    }
}
