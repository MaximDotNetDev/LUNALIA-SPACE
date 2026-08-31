using System;

namespace SchoolJournal.Client.Core.Features.Operations.Quizzes;

public sealed record OpenQuizQuestionsMessage(Guid QuizId);