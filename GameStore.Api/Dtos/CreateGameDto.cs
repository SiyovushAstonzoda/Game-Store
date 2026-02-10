using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos
{
    public record CreateGameDto(
        [Required][StringLength(50)] string Name,
        [Range(1, 50)] int GenreId,
        [Range(0, 1000)] decimal Price,
        [Required] DateOnly ReleaseDate
    );
}