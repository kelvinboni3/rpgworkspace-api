using RpgWorkspace.Application.DTOs.BookVolume;
using RpgWorkspace.Application.Interfaces;
using RpgWorkspace.Domain.Entities;

namespace RpgWorkspace.Application.Services;

public sealed class BookVolumeService : IBookVolumeService
{
    private const long MaxFileSizeBytes = 20 * 1024 * 1024;

    private readonly IBookVolumeRepository _bookVolumeRepository;
    private readonly ICharacterTabBlockRepository _characterTabBlockRepository;
    private readonly ICharacterTabRepository _characterTabRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IMediaAssetRepository _mediaAssetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookVolumeService(
        IBookVolumeRepository bookVolumeRepository,
        ICharacterTabBlockRepository characterTabBlockRepository,
        ICharacterTabRepository characterTabRepository,
        ICharacterRepository characterRepository,
        ICampaignRepository campaignRepository,
        IWorkspaceRepository workspaceRepository,
        IMediaAssetRepository mediaAssetRepository,
        IUnitOfWork unitOfWork)
    {
        _bookVolumeRepository = bookVolumeRepository;
        _characterTabBlockRepository = characterTabBlockRepository;
        _characterTabRepository = characterTabRepository;
        _characterRepository = characterRepository;
        _campaignRepository = campaignRepository;
        _workspaceRepository = workspaceRepository;
        _mediaAssetRepository = mediaAssetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<BookVolumeResponse>> GetAllByBlockAsync(
        Guid characterTabBlockId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(characterTabBlockId, cancellationToken);
        var character = await GetCharacterForBlockOrThrowAsync(block, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanView(character, workspace, requestingUserId, "Book volume not found.");

        var volumes = await _bookVolumeRepository.GetAllByBlockAsync(characterTabBlockId, cancellationToken);
        return volumes.Select(v => ToResponse(v)).ToList();
    }

    public async Task<BookVolumeResponse> UploadAsync(
        Guid characterTabBlockId,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        Stream content,
        Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(characterTabBlockId, cancellationToken);
        var character = await GetCharacterForBlockOrThrowAsync(block, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Book volume not found.");

        if (!string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only PDF files are accepted.");

        if (fileSizeBytes <= 0 || fileSizeBytes > MaxFileSizeBytes)
            throw new ArgumentException($"File size must be between 1 byte and {MaxFileSizeBytes / (1024 * 1024)} MB.");

        var siblings = await _bookVolumeRepository.GetAllByBlockAsync(characterTabBlockId, cancellationToken);
        var nextOrder = siblings.Count == 0 ? 0 : siblings.Max(v => v.Order) + 1;

        using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, cancellationToken);

        var asset = MediaAsset.Create(memoryStream.ToArray(), contentType, originalFileName, fileSizeBytes);
        await _mediaAssetRepository.AddAsync(asset, cancellationToken);

        var volume = BookVolume.Create(characterTabBlockId, nextOrder, asset.Id);
        await _bookVolumeRepository.AddAsync(volume, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(volume, asset);
    }

    public async Task<IReadOnlyList<BookVolumeResponse>> ReorderAsync(
        Guid characterTabBlockId, ReorderBookVolumesRequest request, Guid requestingUserId,
        CancellationToken cancellationToken = default)
    {
        var block = await GetBlockOrThrowAsync(characterTabBlockId, cancellationToken);
        var character = await GetCharacterForBlockOrThrowAsync(block, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Book volume not found.");

        var siblings = await _bookVolumeRepository.GetAllByBlockAsync(characterTabBlockId, cancellationToken);

        if (request.OrderedVolumeIds.Count != siblings.Count
            || !request.OrderedVolumeIds.ToHashSet().SetEquals(siblings.Select(v => v.Id)))
        {
            throw new ArgumentException("Reorder list must contain exactly the current set of volumes.");
        }

        var byId = siblings.ToDictionary(v => v.Id);
        for (var i = 0; i < request.OrderedVolumeIds.Count; i++)
            byId[request.OrderedVolumeIds[i]].Reorder(i);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return siblings.OrderBy(v => v.Order).Select(v => ToResponse(v)).ToList();
    }

    public async Task DeleteAsync(Guid volumeId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var volume = await _bookVolumeRepository.GetByIdAsync(volumeId, cancellationToken)
            ?? throw new KeyNotFoundException("Book volume not found.");

        var block = await GetBlockOrThrowAsync(volume.CharacterTabBlockId, cancellationToken);
        var character = await GetCharacterForBlockOrThrowAsync(block, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanManage(character, workspace, requestingUserId, "Book volume not found.");

        var asset = await _mediaAssetRepository.GetByIdAsync(volume.MediaAssetId, cancellationToken);

        _bookVolumeRepository.Remove(volume);
        if (asset is not null)
            _mediaAssetRepository.Remove(asset);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<(byte[] Content, string ContentType)> GetContentAsync(
        Guid volumeId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var volume = await _bookVolumeRepository.GetByIdAsync(volumeId, cancellationToken)
            ?? throw new KeyNotFoundException("Book volume not found.");

        var block = await GetBlockOrThrowAsync(volume.CharacterTabBlockId, cancellationToken);
        var character = await GetCharacterForBlockOrThrowAsync(block, cancellationToken);
        var workspace = await ResolveWorkspaceAsync(character.CampaignId, cancellationToken);
        CharacterAuthorizationHelper.EnsureCanView(character, workspace, requestingUserId, "Book volume not found.");

        return (volume.MediaAsset.Content, volume.MediaAsset.ContentType);
    }

    private async Task<CharacterTabBlock> GetBlockOrThrowAsync(Guid id, CancellationToken ct)
        => await _characterTabBlockRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Character tab block not found.");

    private async Task<Character> GetCharacterForBlockOrThrowAsync(CharacterTabBlock block, CancellationToken ct)
    {
        var tab = await _characterTabRepository.GetByIdAsync(block.CharacterTabId, ct)
            ?? throw new KeyNotFoundException("Character tab not found.");

        return await _characterRepository.GetByIdAsync(tab.CharacterId, ct)
            ?? throw new KeyNotFoundException("Character not found.");
    }

    private Task<Workspace?> ResolveWorkspaceAsync(Guid? campaignId, CancellationToken ct)
        => CharacterAuthorizationHelper.ResolveWorkspaceAsync(_campaignRepository, _workspaceRepository, campaignId, ct);

    private static BookVolumeResponse ToResponse(BookVolume volume, MediaAsset? assetOverride = null)
    {
        var asset = assetOverride ?? volume.MediaAsset;
        return new BookVolumeResponse(
            volume.Id.ToString(),
            volume.CharacterTabBlockId.ToString(),
            volume.Order,
            asset.OriginalFileName ?? "arquivo.pdf",
            $"/api/book-volumes/{volume.Id}/content",
            asset.FileSizeBytes,
            volume.CreatedAt,
            volume.UpdatedAt);
    }
}
