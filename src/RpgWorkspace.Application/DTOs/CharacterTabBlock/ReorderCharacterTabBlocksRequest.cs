using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.CharacterTabBlock;

public sealed record ReorderCharacterTabBlocksRequest(
    [Required] IReadOnlyList<Guid> OrderedBlockIds
);
