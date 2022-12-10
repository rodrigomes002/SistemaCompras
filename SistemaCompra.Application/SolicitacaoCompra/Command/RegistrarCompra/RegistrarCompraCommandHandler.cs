using MediatR;
using Microsoft.EntityFrameworkCore.Internal;
using SistemaCompra.Domain.Core;
using SistemaCompra.Domain.SolicitacaoCompraAggregate;
using SistemaCompra.Infra.Data.UoW;
using System.Threading;
using System.Threading.Tasks;
using SolicitacaoAgg = SistemaCompra.Domain.SolicitacaoCompraAggregate;


namespace SistemaCompra.Application.SolicitacaoCompra.Command.RegistrarCompra
{
    public class RegistrarCompraCommandHandler : CommandHandler, IRequestHandler<RegistrarCompraCommand, bool>
    {
        private readonly ISolicitacaoCompraRepository solicitacaoRepository;

        public RegistrarCompraCommandHandler(ISolicitacaoCompraRepository solicitacaoRepository, IUnitOfWork uow, IMediator mediator) : base(uow, mediator)
        {
            this.solicitacaoRepository = solicitacaoRepository;
        }

        public Task<bool> Handle(RegistrarCompraCommand request, CancellationToken cancellationToken)
        {
            if (request.Itens.Count == 0)
                throw new BusinessRuleException("A solicitação de compra deve possuir itens!");

            var solicitacaoCompra = new SolicitacaoAgg.SolicitacaoCompra(request.UsuarioSolicitante, request.NomeFornecedor);
            
            Parallel.ForEach(request.Itens, item =>
            {
                solicitacaoCompra.AdicionarItem(item.Produto, item.Qtde);
            });

            solicitacaoRepository.RegistrarCompra(solicitacaoCompra);

            Commit();
            PublishEvents(solicitacaoCompra.Events);

            return Task.FromResult(true);
        }
    }
}
