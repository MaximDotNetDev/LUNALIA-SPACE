using ErrorOr;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using SchoolJournal.Contracts.DTOs.AiGeneration;

namespace SchoolJournal.Application.Common.Interfaces;

public interface IAiQuizGenerator
{
    public Task<ErrorOr<GeneratedQuizResponse>> GenerateMultipleChoiceAsync(
        string text,
        int totalQuestions,
        int multiAnswerCount,
        int judgmentCount,
        int pointsPerQuestion,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<GeneratedQuizResponse>> GenerateFillInTheBlankAsync(
        string text,
        int questionCount,
        int pointsPerQuestion,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<GeneratedQuizResponse>> GenerateMatchingAsync(
            string text,
            int questionCount,
            int pointsPerQuestion,
            CancellationToken cancellationToken = default);

    public Task<ErrorOr<TrueFalseAiResponseDto>> GenerateTrueFalseAsync(
            string text,
            int questionCount,
            CancellationToken cancellationToken = default);

    public Task<ErrorOr<GeneratedQuizResponse>> GenerateOddOneOutAsync(
        string text,
        int questionCount,
        int pointsPerQuestion,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<GeneratedQuizResponse>> GenerateGuessByDescriptionAsync(
        string text,
        int questionCount,
        int pointsPerQuestion,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<GeneratedQuizResponse>> GenerateProofreaderAsync(
        string text,
        int questionCount,
        int pointsPerQuestion,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<GeneratedQuizResponse>> GenerateAssociativeBushAsync(
        string text,
        int questionCount,
        int pointsPerQuestion,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<CrosswordAiResponseDto>> GenerateCrosswordAsync(
        string text,
        int wordCount,
        CancellationToken cancellationToken = default);

    public Task<ErrorOr<FillwordAiResponseDto>> GenerateFillwordAsync(
        string text, 
        int wordCount, 
        CancellationToken cancellationToken = default);
}