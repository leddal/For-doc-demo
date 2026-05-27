using SmartPark.SharedKernel;
using WorkOrderAggregate = SmartPark.WorkOrder.Domain.WorkOrder;
using SmartPark.WorkOrder.Domain;

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
        _ = Assert.Throws<DomainException>(action);
    }
}
