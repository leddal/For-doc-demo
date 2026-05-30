using SmartPark.SharedContracts.Common;
using SmartPark.SharedKernel;
using SmartPark.WorkOrder.Application;
using SmartPark.WorkOrder.Domain;
using WorkOrderAggregate = SmartPark.WorkOrder.Domain.WorkOrder;

namespace SmartPark.WorkOrder.Tests;

public sealed class WorkOrderStatusMachineTests
{
    [Fact]
    public void Should_Advance_Through_Valid_Flow()
    {
        var workOrder = WorkOrderAggregate.Create(
            "WO-TEST-001",
            "测试工单",
            "测试流程流转",
            WorkOrderSourceType.Manual,
            WorkOrderBusinessType.Maintenance,
            WorkOrderPriority.Medium,
            "测试区域",
            null,
            null,
            null,
            "测试上报人",
            "u1",
            "创建人",
            DateTimeOffset.UtcNow);

        workOrder.Dispatch("dispatcher", "调度员", "operator", "处理员", "派单", DateTimeOffset.UtcNow);
        workOrder.Accept("operator", "处理员", "接单", DateTimeOffset.UtcNow);
        workOrder.Arrive("operator", "处理员", "到场", DateTimeOffset.UtcNow);
        workOrder.Start("operator", "处理员", "开始处理", DateTimeOffset.UtcNow);
        workOrder.Complete("operator", "处理员", "处理完成", DateTimeOffset.UtcNow);
        workOrder.Verify("reviewer", "验收员", "验收通过", DateTimeOffset.UtcNow);
        workOrder.Close("reviewer", "验收员", "关闭工单", DateTimeOffset.UtcNow);

        Assert.Equal(WorkOrderStatus.Closed, workOrder.Status);
        Assert.Equal(8, workOrder.ActionLogs.Count);
    }

    [Fact]
    public void Should_Reject_Invalid_Transition()
    {
        var workOrder = WorkOrderAggregate.Create(
            "WO-TEST-002",
            "非法流转工单",
            "测试非法流转",
            WorkOrderSourceType.Manual,
            WorkOrderBusinessType.Maintenance,
            WorkOrderPriority.Low,
            "测试区域",
            null,
            null,
            null,
            "测试上报人",
            "u1",
            "创建人",
            DateTimeOffset.UtcNow);

        Action action = () => workOrder.Arrive("operator", "处理员", "非法到场", DateTimeOffset.UtcNow);
        var exception = Assert.Throws<DomainException>(action);

        Assert.Equal("work_order_invalid_status_transition", exception.Code);
        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Throw_NotFoundException_When_WorkOrder_Does_Not_Exist()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Create_Should_Throw_ValidationException_When_Request_Is_Invalid()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(
            new CreateWorkOrderRequest(" ", string.Empty, (WorkOrderSourceType)99, (WorkOrderBusinessType)0, (WorkOrderPriority)0, " ", null, null, null, null, null),
            new ActionContext("u1", "创建人")));
    }

    private static WorkOrderService CreateService(params WorkOrderAggregate[] workOrders)
        => new(new FakeRepository(workOrders), [new CreateWorkOrderRequestValidator()], [new DispatchWorkOrderRequestValidator()], [new ActionRequestValidator()]);

    private sealed class FakeRepository(params WorkOrderAggregate[] workOrders) : IWorkOrderRepository
    {
        private readonly List<WorkOrderAggregate> _workOrders = [.. workOrders];

        public Task AddAsync(WorkOrderAggregate entity, CancellationToken cancellationToken = default)
        {
            _workOrders.Add(entity);
            return Task.CompletedTask;
        }

        public Task<WorkOrderAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_workOrders.FirstOrDefault(item => item.Id == id));

        public Task<PagedResult<WorkOrderAggregate>> QueryAsync(WorkOrderQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(new PagedResult<WorkOrderAggregate>(_workOrders, _workOrders.Count, query.PageNumber, query.PageSize));

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
