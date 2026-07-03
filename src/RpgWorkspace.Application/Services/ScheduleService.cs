using RpgWorkspace.Application.DTOs.Schedule;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly IScheduleEventRepository _scheduleEventRepository;
    private readonly IScheduleResponseRepository _scheduleResponseRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ScheduleService(
        IScheduleEventRepository scheduleEventRepository,
        IScheduleResponseRepository scheduleResponseRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleEventRepository = scheduleEventRepository;
        _scheduleResponseRepository = scheduleResponseRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ScheduleEventResponse>> GetAllByCampaignAsync(
        Guid campaignId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var events = await _scheduleEventRepository.GetAllByCampaignAsync(campaignId, cancellationToken);

        return events.Select(ToEventResponse).ToList();
    }

    public async Task<ScheduleEventResponse> GetByIdAsync(
        Guid scheduleEventId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var scheduleEvent = await GetScheduleEventOrThrowAsync(scheduleEventId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(scheduleEvent.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        return ToEventResponse(scheduleEvent);
    }

    public async Task<ScheduleEventResponse> CreateAsync(
        Guid campaignId, CreateScheduleEventRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await GetCampaignOrThrowAsync(campaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        var scheduleEvent = ScheduleEvent.Create(
            campaignId,
            request.Title,
            request.Description,
            request.ProposedDate,
            request.Status,
            requestingUserId);

        await _scheduleEventRepository.AddAsync(scheduleEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToEventResponse(scheduleEvent);
    }

    public async Task<ScheduleEventResponse> UpdateAsync(
        Guid scheduleEventId, UpdateScheduleEventRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var scheduleEvent = await GetScheduleEventOrThrowAsync(scheduleEventId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(scheduleEvent.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        scheduleEvent.Update(request.Title, request.Description, request.ProposedDate, request.Status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToEventResponse(scheduleEvent);
    }

    public async Task DeleteAsync(
        Guid scheduleEventId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var scheduleEvent = await GetScheduleEventOrThrowAsync(scheduleEventId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(scheduleEvent.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsOwnerOrMaster(workspace, requestingUserId);

        _scheduleEventRepository.Remove(scheduleEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ScheduleResponseResponse> RespondAsync(
        Guid scheduleEventId, CreateScheduleResponseRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var scheduleEvent = await GetScheduleEventOrThrowAsync(scheduleEventId, cancellationToken);
        var campaign = await GetCampaignOrThrowAsync(scheduleEvent.CampaignId, cancellationToken);
        var workspace = await GetWorkspaceWithMembersOrThrowAsync(campaign.WorkspaceId, cancellationToken);
        EnsureIsMember(workspace, requestingUserId);

        var existingResponse = await _scheduleResponseRepository.GetByEventAndUserAsync(
            scheduleEventId, requestingUserId, cancellationToken);

        if (existingResponse is not null)
        {
            existingResponse.Update(request.Response, request.Comment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ToScheduleResponse(existingResponse);
        }

        var scheduleResponse = ScheduleResponse.Create(
            scheduleEventId,
            requestingUserId,
            request.Response,
            request.Comment);

        await _scheduleResponseRepository.AddAsync(scheduleResponse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToScheduleResponse(scheduleResponse);
    }

    private async Task<ScheduleEvent> GetScheduleEventOrThrowAsync(Guid id, CancellationToken ct)
        => await _scheduleEventRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Schedule event not found.");

    private async Task<Campaign> GetCampaignOrThrowAsync(Guid id, CancellationToken ct)
        => await _campaignRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Campaign not found.");

    private async Task<Workspace> GetWorkspaceWithMembersOrThrowAsync(Guid id, CancellationToken ct)
        => await _workspaceRepository.GetByIdWithMembersAsync(id, ct)
            ?? throw new KeyNotFoundException("Workspace not found.");

    private static void EnsureIsMember(Workspace workspace, Guid userId)
    {
        if (!workspace.IsMember(userId))
            throw new KeyNotFoundException("Schedule event not found.");
    }

    private static void EnsureIsOwnerOrMaster(Workspace workspace, Guid userId)
    {
        if (!workspace.IsOwnerOrMaster(userId))
            throw new UnauthorizedAccessException("Only Owner or Master can perform this action.");
    }

    private static ScheduleEventResponse ToEventResponse(ScheduleEvent e) =>
        new(
            e.Id.ToString(),
            e.CampaignId.ToString(),
            e.Title,
            e.Description,
            e.ProposedDate,
            e.Status,
            e.CreatedByUserId.ToString(),
            e.Responses.Select(ToScheduleResponse).ToList(),
            e.CreatedAt,
            e.UpdatedAt);

    private static ScheduleResponseResponse ToScheduleResponse(ScheduleResponse r) =>
        new(
            r.Id.ToString(),
            r.ScheduleEventId.ToString(),
            r.UserId.ToString(),
            r.Response,
            r.Comment,
            r.CreatedAt,
            r.UpdatedAt);
}
