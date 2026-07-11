namespace RpgWorkspace.Application.DTOs.BookVolume;

public sealed record ReorderBookVolumesRequest(IReadOnlyList<Guid> OrderedVolumeIds);
