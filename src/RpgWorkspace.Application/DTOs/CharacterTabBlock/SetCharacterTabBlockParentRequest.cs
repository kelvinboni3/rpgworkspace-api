namespace RpgWorkspace.Application.DTOs.CharacterTabBlock;

/// <summary>Move um bloco existente para dentro de um bloco-container (Registro expansível/Imagem) ou de volta pro topo da aba (null).</summary>
public sealed record SetCharacterTabBlockParentRequest(
    Guid? ParentBlockId
);
