using EduApoyos.Application.Common.Results;
using MediatR;

namespace EduApoyos.Application.Features.Students.Delete;

/// <summary>
/// Removes a student from the platform (US-010). The command produces no payload on success;
/// the HTTP layer maps the successful result to a <c>204 No Content</c>.
/// </summary>
public sealed record DeleteStudentCommand(Guid Id) : IRequest<Result>;
