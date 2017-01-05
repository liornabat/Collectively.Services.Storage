using System.Threading.Tasks;
using Coolector.Common.Events;
using Coolector.Common.Services;
using Coolector.Services.Remarks.Shared.Events;
using RawRabbit;
using Coolector.Services.Storage.Repositories;
using System.Linq;

namespace Coolector.Services.Storage.Handlers
{
    public class RemarkVoteDeletedHandler : IEventHandler<RemarkVoteDeleted>
    {
        private readonly IHandler _handler;
        private readonly IBusClient _bus;
        private readonly IRemarkRepository _remarkRepository;

        public RemarkVoteDeletedHandler(IHandler handler, IBusClient bus, IRemarkRepository remarkRepository)
        {
            _handler = handler;
            _bus = bus;
            _remarkRepository = remarkRepository;
        }

        public async Task HandleAsync(RemarkVoteDeleted @event)
        {
            await _handler
                .Run(async () => 
                {
                    var remark = await _remarkRepository.GetByIdAsync(@event.RemarkId);
                    if (remark.HasNoValue)
                    {
                        return;
                    }

                    var vote = remark.Value.Votes
                        .SingleOrDefault(x => x.UserId == @event.UserId);

                    if (vote.Positive)
                    {
                        remark.Value.Rating--;
                    }
                    else
                    {
                        remark.Value.Rating++;
                    }
                    remark.Value.Votes.Remove(vote);
                    await _remarkRepository.UpdateAsync(remark.Value);
                })
                .ExecuteAsync();
        }
    }
}