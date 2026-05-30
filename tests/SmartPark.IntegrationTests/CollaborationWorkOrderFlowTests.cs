using SmartPark.Collaboration.Application;
using SmartPark.Collaboration.Domain;
using SmartPark.SharedContracts.Common;
using SmartPark.SharedKernel;

namespace SmartPark.IntegrationTests;

public sealed class CollaborationWorkOrderFlowTests
{
    [Fact]
    public async Task Create_WorkOrder_From_Event_Should_Update_Event_Status()
    {
        var repository = new FakeRepository();
        var service = CreateService(repository, new FakeWorkOrderGateway());

        var created = await service.CreateEventAsync(new CreateEventRequest(
            "事件测试",
            "需要生成工单",
            EventSeverity.High,
            "东门片区",
            null,
            null));

        var workOrder = await service.CreateWorkOrderAsync(created.Id, new CreateWorkOrderFromEventRequest(), CancellationToken.None);
        var eventItem = await service.GetEventAsync(created.Id, CancellationToken.None);

        Assert.Equal(EventStatus.WorkOrderCreated, eventItem.Status);
        Assert.Equal(workOrder.Id, eventItem.WorkOrderId);
    }

    [Fact]
    public async Task Create_WorkOrder_From_Missing_Event_Should_Throw_NotFoundException()
    {
        var service = CreateService(new FakeRepository(), new FakeWorkOrderGateway());

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateWorkOrderAsync(Guid.NewGuid(), new CreateWorkOrderFromEventRequest()));
    }

    private static CollaborationService CreateService(ICollaborationRepository repository, IWorkOrderGateway gateway)
        => new(
            repository,
            gateway,
            [new CreateEventRequestValidator()],
            [new CloseEventRequestValidator()],
            [new CreateWorkOrderFromEventRequestValidator()],
            [new CreateAnnouncementRequestValidator()]);

    private sealed class FakeRepository : ICollaborationRepository
    {
        private readonly List<EventRecord> _events = [];
        private readonly List<Announcement> _announcements = [];

        public Task AddEventAsync(EventRecord entity, CancellationToken cancellationToken = default)
        {
            _events.Add(entity);
            return Task.CompletedTask;
        }

        public Task<EventRecord?> GetEventAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_events.FirstOrDefault(item => item.Id == id));

        public Task<PagedResult<EventRecord>> QueryEventsAsync(EventQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(new PagedResult<EventRecord>(_events, _events.Count, query.PageNumber, query.PageSize));

        public Task AddAnnouncementAsync(Announcement entity, CancellationToken cancellationToken = default)
        {
            _announcements.Add(entity);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Announcement>> GetAnnouncementsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Announcement>>(_announcements);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeWorkOrderGateway : IWorkOrderGateway
    {
        public Task<CreatedWorkOrderInfo> CreateFromEventAsync(EventRecord entity, CreateWorkOrderFromEventRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new CreatedWorkOrderInfo(Guid.NewGuid(), "WO-FAKE-001"));
    }
}
