using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Grade.CreateGrade;

public sealed class CreateGradeCommandHandler(
    IGradeRepository gradeRepository,
    ILessonRepository lessonRepository,
    IWalletRepository walletRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<CreateGradeCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateGradeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        if (currentRole == RoleType.Teacher)
        {
            var isOwner = await lessonRepository.VerifyLessonOwnershipAsync(request.LessonId, currentUserId, cancellationToken).ConfigureAwait(false);
            if (!isOwner)
            {
                return Error.Forbidden(
                    code: "Grade.Forbidden",
                    description: "Ви не маєте права виставляти оцінки за урок, який ви не ведете.");
            }
        }

        var grade = new Domain.Entities.Operations.Grade
        {
            LessonId = request.LessonId,
            StudentId = request.StudentId,
            GradeValue = request.GradeValue,
            Comment = request.Comment,
            GradeTypeId = request.GradeTypeId,
            CreatedByUserId = currentUserId,
            UpdatedByUserId = currentUserId
        };

        var gradeId = await gradeRepository.AddAsync(grade, cancellationToken).ConfigureAwait(false);

        if (int.TryParse(request.GradeValue, out int earnedCoins) && earnedCoins > 0)
        {
            var subjectId = await lessonRepository.GetSubjectIdByLessonAsync(request.LessonId, cancellationToken).ConfigureAwait(false);
            var wallet = await walletRepository.GetWalletAsync(request.StudentId, subjectId, cancellationToken).ConfigureAwait(false);
            Guid targetWalletId;

            if (wallet is null)
            {
                targetWalletId = await walletRepository.CreateWalletAsync(new Domain.Entities.Operations.Wallet
                {
                    StudentId = request.StudentId,
                    SubjectId = subjectId,
                    Balance = earnedCoins
                }, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                targetWalletId = wallet.WalletId;
                await walletRepository.UpdateBalanceAsync(wallet.WalletId, wallet.Balance + earnedCoins, [.. wallet.RowVersion], cancellationToken).ConfigureAwait(false);
            }

            await walletRepository.RecordTransactionAsync(new Domain.Entities.Operations.CoinTransaction
            {
                WalletId = targetWalletId,
                Amount = earnedCoins,
                ReferenceId = gradeId,
                TransactionType = "Earned_Grade"
            }, cancellationToken).ConfigureAwait(false);
        }

        var newState = await gradeRepository.GetByIdAsync(gradeId, cancellationToken).ConfigureAwait(false);
        if (newState is null) return Error.Unexpected(description: "Сталася помилка при отриманні створеної оцінки.");

        auditContext.TrackNewState(newState);
        return gradeId;
    }
}