using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.NoteStructuring;

public sealed record StructureNoteRequest(
    [Required, MaxLength(4000)] string NoteText
);
