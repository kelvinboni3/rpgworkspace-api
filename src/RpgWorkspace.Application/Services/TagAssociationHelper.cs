using RpgWorkspace.Application.DTOs.Tag;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

internal static class TagAssociationHelper
{
    public static async Task ValidateTagsBelongToCampaignAsync(
        ITagRepository tagRepository,
        Guid campaignId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        var distinctTagIds = tagIds.Distinct().ToList();
        if (distinctTagIds.Count == 0)
            return;

        var tags = await tagRepository.GetByIdsAsync(distinctTagIds, cancellationToken);

        if (tags.Count != distinctTagIds.Count || tags.Any(t => t.CampaignId != campaignId))
            throw new InvalidOperationException("All tags must exist and belong to the same campaign.");
    }

    public static IReadOnlyList<TagResponse> ToResponses(IEnumerable<Tag> tags)
        => tags
            .OrderBy(t => t.Name)
            .Select(t => new TagResponse(t.Id.ToString(), t.CampaignId.ToString(), t.Name, t.Color))
            .ToList();
}
