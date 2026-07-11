using RpgWorkspace.Domain.Common;

namespace RpgWorkspace.Domain.Entities;

public sealed class CharacterTabBlockLink : BaseEntity
{
    public Guid SourceBlockId { get; private set; }
    public Guid TargetBlockId { get; private set; }

    private CharacterTabBlockLink() { }

    public static CharacterTabBlockLink Create(Guid sourceBlockId, Guid targetBlockId) =>
        new() { SourceBlockId = sourceBlockId, TargetBlockId = targetBlockId };
}
