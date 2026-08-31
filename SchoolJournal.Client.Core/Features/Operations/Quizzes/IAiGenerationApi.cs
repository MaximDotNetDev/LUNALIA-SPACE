using Refit;
using System.Diagnostics.CodeAnalysis;
using SchoolJournal.Contracts.DTOs.AiGeneration;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Client.Core.Features.Operations.Quizzes;

public interface IAiGenerationApi
{
    [Post("/api/ai/generate-multiple-choice")]
    public Task<IApiResponse<GeneratedQuizResponse>> GenerateMultipleChoiceAsync(
        [Body] GenerateMultipleChoiceRequest request,
        CancellationToken ct = default);

    [Multipart]
    [Post("/api/ai/generate-multiple-choice-from-pdf")]
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Required by Refit for multipart form mapping.")]
    public Task<IApiResponse<GeneratedQuizResponse>> GenerateMultipleChoiceFromPdfAsync(
            StreamPart file,
            [Query, AliasAs("TotalQuestions")] int totalQuestions,
            [Query, AliasAs("MultiAnswerCount")] int multiAnswerCount,
            [Query, AliasAs("JudgmentCount")] int judgmentCount,
            [Query, AliasAs("PointsPerQuestion")] int pointsPerQuestion,
[Query, AliasAs("StartPage")] int? startPage = null,
            [Query, AliasAs("EndPage")] int? endPage = null,
            CancellationToken ct = default);

    [Multipart]
    [Post("/api/ai/generate-true-false-from-pdf")]
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Required by Refit for multipart form mapping.")]
    public Task<IApiResponse<GeneratedQuizResponse>> GenerateTrueFalseFromPdfAsync(
            StreamPart file,
            [Query, AliasAs("QuestionCount")] int questionCount,
            [Query, AliasAs("PointsPerQuestion")] int pointsPerQuestion,
            [Query, AliasAs("StartPage")] int? startPage = null,
            [Query, AliasAs("EndPage")] int? endPage = null,
            CancellationToken ct = default);

    [Multipart]
    [Post("/api/ai/generate-odd-one-out-from-pdf")]
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Required by Refit for multipart form mapping.")]
    public Task<IApiResponse<GeneratedQuizResponse>> GenerateOddOneOutFromPdfAsync(
            StreamPart file,
            [Query, AliasAs("QuestionCount")] int questionCount,
            [Query, AliasAs("PointsPerQuestion")] int pointsPerQuestion,
            [Query, AliasAs("StartPage")] int? startPage = null,
            [Query, AliasAs("EndPage")] int? endPage = null,
            CancellationToken ct = default);

    [Multipart]
    [Post("/api/ai/generate-guess-by-description-from-pdf")]
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Required by Refit for multipart form mapping.")]
    public Task<IApiResponse<GeneratedQuizResponse>> GenerateGuessByDescriptionFromPdfAsync(
            StreamPart file,
            [Query, AliasAs("QuestionCount")] int questionCount,
            [Query, AliasAs("PointsPerQuestion")] int pointsPerQuestion,
            [Query, AliasAs("StartPage")] int? startPage = null,
            [Query, AliasAs("EndPage")] int? endPage = null,
            CancellationToken ct = default);

    [Multipart]
    [Post("/api/ai/generate-proofreader-from-pdf")]
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Required by Refit for multipart form mapping.")]
    public Task<IApiResponse<GeneratedQuizResponse>> GenerateProofreaderFromPdfAsync(
            StreamPart file,
            [Query, AliasAs("QuestionCount")] int questionCount,
            [Query, AliasAs("PointsPerQuestion")] int pointsPerQuestion,
            [Query, AliasAs("StartPage")] int? startPage = null,
            [Query, AliasAs("EndPage")] int? endPage = null,
            CancellationToken ct = default);

    [Multipart]
    [Post("/api/ai/generate-associative-bush-from-pdf")]
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Required by Refit for multipart form mapping.")]
    public Task<IApiResponse<GeneratedQuizResponse>> GenerateAssociativeBushFromPdfAsync(
            StreamPart file,
            [Query, AliasAs("QuestionCount")] int questionCount,
            [Query, AliasAs("PointsPerQuestion")] int pointsPerQuestion,
            [Query, AliasAs("StartPage")] int? startPage = null,
            [Query, AliasAs("EndPage")] int? endPage = null,
            CancellationToken ct = default);

    [Multipart]
    [Post("/api/ai/generate-crossword-from-pdf")]
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Required by Refit for multipart form mapping.")]
    public Task<IApiResponse<GeneratedQuizResponse>> GenerateCrosswordFromPdfAsync(
            StreamPart file,
            [Query, AliasAs("WordCount")] int wordCount,
            [Query, AliasAs("PointsPerWord")] int pointsPerWord,
            [Query, AliasAs("StartPage")] int? startPage = null,
            [Query, AliasAs("EndPage")] int? endPage = null,
            CancellationToken ct = default);

    [Multipart]
    [Post("/api/ai/generate-fillword-from-pdf")]
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Required by Refit for multipart form mapping.")]
    public Task<IApiResponse<GeneratedQuizResponse>> GenerateFillwordFromPdfAsync(
            StreamPart file,
            [Query, AliasAs("WordCount")] int wordCount,
            [Query, AliasAs("PointsPerWord")] int pointsPerWord,
            [Query, AliasAs("StartPage")] int? startPage = null,
            [Query, AliasAs("EndPage")] int? endPage = null,
            CancellationToken ct = default);

    [Multipart]
    [Post("/api/ai/generate-fill-blanks-from-pdf")]
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Required by Refit for multipart form mapping.")]
    public Task<IApiResponse<GeneratedQuizResponse>> GenerateFillInTheBlankFromPdfAsync(
            StreamPart file,
            [Query, AliasAs("QuestionCount")] int questionCount,
            [Query, AliasAs("PointsPerQuestion")] int pointsPerQuestion,
            [Query, AliasAs("StartPage")] int? startPage = null,
            [Query, AliasAs("EndPage")] int? endPage = null,
            CancellationToken ct = default);

    [Multipart]
    [Post("/api/ai/generate-matching-from-pdf")]
    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Required by Refit for multipart form mapping.")]
    public Task<IApiResponse<GeneratedQuizResponse>> GenerateMatchingFromPdfAsync(
            StreamPart file,
            [Query, AliasAs("QuestionCount")] int questionCount,
            [Query, AliasAs("PointsPerQuestion")] int pointsPerQuestion,
            [Query, AliasAs("StartPage")] int? startPage = null,
            [Query, AliasAs("EndPage")] int? endPage = null,
            CancellationToken ct = default);
}