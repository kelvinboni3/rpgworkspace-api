using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.NoteStructuring;

public sealed record ImportSheetRequest(
    [Required, MaxLength(4000)] string SheetText
);
