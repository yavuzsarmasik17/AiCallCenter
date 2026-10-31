using AiCallCenter.Application.KnowledgeBases.DTOs;
using AiCallCenter.Application.KnowledgeBases.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.KnowledgeBases.Commands.CreateKnowledgeBase;

public sealed class CreateKnowledgeBaseCommandHandler
    : IRequestHandler<
        CreateKnowledgeBaseCommand,
        KnowledgeBaseDto>
{
    private readonly IKnowledgeBaseRepository
        _knowledgeBaseRepository;

    public CreateKnowledgeBaseCommandHandler(
        IKnowledgeBaseRepository knowledgeBaseRepository)
    {
        _knowledgeBaseRepository = knowledgeBaseRepository;
    }

    public async Task<KnowledgeBaseDto> Handle(
        CreateKnowledgeBaseCommand request,
        CancellationToken cancellationToken)
    {
        var companyExists =
            await _knowledgeBaseRepository.CompanyExistsAsync(
                request.CompanyId,
                cancellationToken);

        if (!companyExists)
        {
            throw new KeyNotFoundException(
                "Belirtilen firma bulunamadı.");
        }

        var normalizedName = request.Name.Trim();

        var nameExists =
            await _knowledgeBaseRepository.NameExistsAsync(
                request.CompanyId,
                normalizedName,
                cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Bu firmada aynı isimde bir bilgi bankası bulunuyor.");
        }

        var knowledgeBase = new KnowledgeBase(
            request.CompanyId,
            normalizedName,
            request.Description);

        await _knowledgeBaseRepository.AddAsync(
            knowledgeBase,
            cancellationToken);

        await _knowledgeBaseRepository.SaveChangesAsync(
            cancellationToken);

        return new KnowledgeBaseDto
        {
            Id = knowledgeBase.Id,
            CompanyId = knowledgeBase.CompanyId,
            Name = knowledgeBase.Name,
            Description = knowledgeBase.Description,
            IsActive = knowledgeBase.IsActive,
            CreatedAt = knowledgeBase.CreatedAt
        };
    }
}