using RpgWorkspace.Application.DTOs.CharacterExport;

namespace RpgWorkspace.Application.Interfaces;

public interface ICharacterExportService
{
    Task<MarkdownExportResult> ExportMarkdownAsync(
        Guid characterId,
        Guid requestingUserId,
        CancellationToken cancellationToken = default);
}
