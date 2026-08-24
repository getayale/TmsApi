using MediatR;

public record DeleteStudentCommand(int id):IRequest<bool>;