using System.ComponentModel.DataAnnotations;

namespace RpgWorkspace.Application.DTOs.NoteStructuring;

public sealed record StructureNoteRequest(
    [Required, MaxLength(4000)] string NoteText,
    // Aba escolhida pelo jogador no widget da Davena para o relato principal desta anotação
    // (null = deixar a IA decidir). É uma preferência confiável da interface, não vem do texto.
    [MaxLength(120)] string? PreferredTabName = null
);
